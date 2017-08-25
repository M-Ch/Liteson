﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Liteson
{
	internal static class TypeReader
	{
		private static readonly TypeInfo EnumerableType = typeof(IEnumerable).GetTypeInfo();

		public static Func<JsonReader, object> ForType(Type type, Func<Type, TypeDescriptor> descriptorSource)
		{
			var nullable = Nullable.GetUnderlyingType(type);
			if(nullable != null)
				return ForNullable(nullable, descriptorSource);

			if (type.IsEnum)
				return ForEnum(type, descriptorSource);

			return EnumerableType.IsAssignableFrom(type.GetTypeInfo())
				? ForCollection(type, descriptorSource)
				: ForComplex(type, descriptorSource);
		}

		private static Func<JsonReader, object> ForEnum(Type type, Func<Type, TypeDescriptor> descriptorSource)
		{
			var underlyingType = Enum.GetUnderlyingType(type);
			var descriptor = descriptorSource(underlyingType);
			var enumValues = new Dictionary<string, object>();
			var toUnderlying = ReflectionUtils.BuildCaster(underlyingType);

			foreach(var value in Enum.GetValues(type))
			{
				enumValues[value.ToString()] = value;
				enumValues[toUnderlying(value).ToString()] = value;
			}

			return reader =>
			{
				if (reader.PeekToken().HasFlag(JsonToken.Number))
					return Enum.ToObject(type, descriptor.Reader(reader));

				var bufferPart = new BufferPart();
				var token = reader.Read(ref bufferPart, out var text);
				if (token != JsonToken.String)
					throw Exceptions.BadToken(reader, token, JsonToken.String | JsonToken.Number);

				return enumValues.TryGetValue(text, out var value)
					? value
					: throw new JsonException($"Unknown enum value '{text}' near line {reader.Line}, column {reader.Column}.");
			};
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
			var info = type.GetTypeInfo();
			if(info.IsClass && info.GetConstructor(Array.Empty<Type>()) == null)
				return reader => throw new JsonException($"Type {type} must define public parameterless constructor.");

			var properties = info
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(i => i.SetMethod != null && i.GetMethod != null)
				.Select(i => new {Property = (MemberInfo)i, Setter = ReflectionUtils.BuildSetter(i), Descriptor = descriptorSource(i.PropertyType)});

			var fields = info
				.GetFields(BindingFlags.Instance | BindingFlags.Public)
				.Select(i => new {Property = (MemberInfo)i, Setter = ReflectionUtils.BuildFieldSetter(i), Descriptor = descriptorSource(i.FieldType)});

			var all = properties
				.Concat(fields)
				.SelectMany(i => JsonNames(i.Property).Select(j => new {Name = j, i.Setter, i.Descriptor}))
				.GroupBy(i => i.Name)
				.ToDictionary(i => i.Key, i => i.First());

			var constructor = info.IsClass
				? ReflectionUtils.BuildConstructor(type)
				: () => Activator.CreateInstance(typeof(Box<>).MakeGenericType(type));

			//struct types are internally wrapped into Box<T> type (see ReflectionUtils)
			var unwrapper = !info.IsClass
				? new Func<object, object>(i => ((IBox) i).Value)
				: null;

			return reader =>
			{
				var bufferPart = new BufferPart();
				var token = reader.Read(ref bufferPart, out var _);
				if (token == JsonToken.Null)
					return info.IsClass ? (object)null : throw new JsonException($"Unable to assign null value to struct type near line {reader.Line}, column {reader.Column}.");
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
			var customName = property.GetCustomAttribute<JsonPropertyAttribute>()?.Name;
			if (!string.IsNullOrEmpty(customName))
			{
				yield return customName;
				yield break;
			}

			yield return property.Name;
			yield return CamelCase.ToCamelCase(property.Name);
			yield return property.Name.ToLower();
		}
	}
}