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
			return EnumerableType.IsAssignableFrom(type.GetTypeInfo())
				? throw new NotImplementedException("collections to do")
				: ForComplex(type, descriptorSource);
		}

		private static Func<JsonReader, object> ForComplex(Type type, Func<Type, TypeDescriptor> descriptorSource)
		{
			var properties = type
				.GetRuntimeProperties()
				.Where(i => i.SetMethod != null && i.GetMethod != null)
				.Select(i => new {Property = i, Setter = ReflectionUtils.BuildSetter(i), Descriptor = descriptorSource(i.PropertyType)})
				.SelectMany(i => JsonNames(i.Property).Select(j => new {Name = j, i.Setter, i.Descriptor}))
				.GroupBy(i => i.Name)
				.ToDictionary(i => i.Key, i => i.First());

			if (type.IsClass && type.GetConstructor(Array.Empty<Type>()) == null)
				throw new JsonException($"Type {type} must define public parameterless constructor.");

			var constructor = type.IsClass
				? ReflectionUtils.BuildConstructor(type)
				: () => Activator.CreateInstance(typeof(Box<>).MakeGenericType(type));
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

					var hasProperty = properties.TryGetValue(propertyName, out var property);
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

		private static IEnumerable<string> JsonNames(PropertyInfo property)
		{
			yield return property.Name;
			yield return property.Name.ToLower();
		}
	}
}