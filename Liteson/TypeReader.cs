using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Liteson
{
	internal static class TypeReader
	{
		private static readonly TypeInfo EnumerableType = typeof(IEnumerable).GetTypeInfo();

		public static Func<JsonReader, object> ForType(Type type, TypeOptions options, Func<Type, TypeDescriptor> descriptorSource)
		{
			var underlyingType = Nullable.GetUnderlyingType(type);
			if(underlyingType != null)
				return ForNullable(underlyingType, descriptorSource);

			return EnumerableType.IsAssignableFrom(type.GetTypeInfo())
				? ForCollection(type, descriptorSource)
				: ForComplex(type, descriptorSource);
		}

		private static Func<JsonReader, object> ForNullable(Type underlyingType, Func<Type, TypeDescriptor> descriptorSource)
		{
			var descriptor = descriptorSource(underlyingType);
			return reader =>
			{
				if (reader.PeekToken() != JsonToken.Null)
					return descriptor.Reader(reader);

				var bufferPart = new BufferPart();
				var token = reader.Read(ref bufferPart, out var _);
				if (token != JsonToken.Null)
					throw Exceptions.BadToken(reader, token, JsonToken.Null);

				return null;
			};
		}

		private static Func<JsonReader, object> ForCollection(Type type, Func<Type, TypeDescriptor> descriptorSource)
		{
			var element = ReflectionUtils.FindCollectionElementType(type);
			var constructor = ReflectionUtils.BuildConstructor(typeof(List<>).MakeGenericType(element));
			var elementDescriptor = descriptorSource(element);

			return reader =>
			{
				var bufferPart = new BufferPart();
				var token = reader.Read(ref bufferPart, out var _);
				if (token == JsonToken.Null)
					return null;
				var target = (IList) constructor();
				if(token != JsonToken.ArrayStart)
					throw Exceptions.BadToken(reader, token, JsonToken.ObjectStart);

				var finalizer = type.IsArray
					? new Func<IList, object>(i =>
					{
						var array = Array.CreateInstance(element, i.Count);
						i.CopyTo(array, 0);
						return array;
					})
					: null;

				var isFirst = true;
				while (true)
				{
					if (reader.PeekToken() == JsonToken.ArrayEnd)
					{
						reader.Read(ref bufferPart, out var _);
						return finalizer != null ? finalizer(target) : target;
					}
					if (!isFirst)
					{
						token = reader.Read(ref bufferPart, out var _);
						if (token != JsonToken.ValueSeparator)
							throw Exceptions.BadToken(reader, token, JsonToken.ValueSeparator);
					}

					var item = elementDescriptor.Reader(reader);
					target.Add(item);

					isFirst = false;
				}
			};
		}

		private static Func<JsonReader, object> ForComplex(Type type, Func<Type, TypeDescriptor> descriptorSource)
		{
			if(type.IsClass && type.GetConstructor(Array.Empty<Type>()) == null)
				return reader => throw new JsonException($"Type {type} must define public parameterless constructor.");

			var properties = type
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(i => i.SetMethod != null && i.GetMethod != null)
				.Select(i => new {Property = (MemberInfo)i, Setter = ReflectionUtils.BuildSetter(i), Descriptor = descriptorSource(i.PropertyType)});

			var fields = type
				.GetFields(BindingFlags.Instance | BindingFlags.Public)
				.Select(i => new {Property = (MemberInfo)i, Setter = ReflectionUtils.BuildFieldSetter(i), Descriptor = descriptorSource(i.FieldType)});

			var all = properties
				.Concat(fields)
				.SelectMany(i => JsonNames(i.Property).Select(j => new {Name = j, i.Setter, i.Descriptor}))
				.GroupBy(i => i.Name)
				.ToDictionary(i => i.Key, i => i.First());

			var constructor = type.IsClass
				? ReflectionUtils.BuildConstructor(type)
				: () => Activator.CreateInstance(typeof(Box<>).MakeGenericType(type));

			//struct types are internally wrapped into Box<T> type (see ReflectionUtils)
			var unwrapper = !type.IsClass
				? new Func<object, object>(i => ((IBox) i).Value)
				: null;

			return reader =>
			{
				var bufferPart = new BufferPart();
				var token = reader.Read(ref bufferPart, out var _);
				if (token == JsonToken.Null)
					return type.IsClass ? (object)null : throw new JsonException($"Unable to assign null value to struct type near line {reader.Line}, column {reader.Column}.");
				if (token != JsonToken.ObjectStart)
					throw Exceptions.BadToken(reader, token, JsonToken.ObjectStart);
				var target = constructor();
				while (true)
				{
					token = reader.Read(ref bufferPart, out var propertyName);
					if (token == JsonToken.ObjectEnd)
						return unwrapper != null ? unwrapper(target) : target;
					if (token != JsonToken.String)
						throw Exceptions.BadToken(reader, token, JsonToken.String);

					var hasProperty = all.TryGetValue(propertyName, out var property);
					token = reader.Read(ref bufferPart, out var _);
					if (token != JsonToken.NameSeparator)
						throw Exceptions.BadToken(reader, token, JsonToken.NameSeparator);

					if (hasProperty)
						property.Setter(target, property.Descriptor.Reader(reader));
					else
						ValueSkipper.SkipNext(reader);

					token = reader.Read(ref bufferPart, out var _);
					if (token == JsonToken.ObjectEnd)
						return unwrapper != null ? unwrapper(target) : target;
					if(token != JsonToken.ValueSeparator)
						throw Exceptions.BadToken(reader, token, JsonToken.ValueSeparator);
				}
			};
		}

		private static IEnumerable<string> JsonNames(MemberInfo property)
		{
			yield return property.Name;
			yield return property.Name.ToLower();
		}
	}
}