﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Liteson
{
	internal static class TypeWriter
	{
		private static readonly TypeInfo EnumerableType = typeof(IEnumerable).GetTypeInfo();

		public static Action<object, SerializationContext> ForType(Type type, Func<Type, TypeDescriptor> descriptorSource)
		{
			var underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null)
				return ForNullable(underlyingType, descriptorSource);

			return EnumerableType.IsAssignableFrom(type.GetTypeInfo())
				? ForCollection(type, descriptorSource)
				: ForComplex(type, descriptorSource);
		}

		private static Action<object, SerializationContext> ForNullable(Type underlyingType, Func<Type, TypeDescriptor> descriptorSource)
		{
			var descriptor = descriptorSource(underlyingType);
			return (obj, context) =>
			{
				if (obj != null)
					descriptor.Writer(obj, context);
				else
					context.Writer.WriteNull();
			};
		}

		private static Action<object, SerializationContext> ForCollection(Type root, Func<Type, TypeDescriptor> descriptorSource)
		{
			var elementType = ReflectionUtils.FindCollectionElementType(root);
			var descriptor = descriptorSource(elementType);

			return (obj, context) =>
			{
				var writer = context.Writer;
				if (obj == null)
				{
					writer.WriteNull();
					return;
				}
				writer.BeginArray();
				foreach(var item in (IEnumerable)obj)
				{
					writer.ArrayItem();
					descriptor.Writer(item, context);
				}
				writer.EndArray();
			};
		}

		private static Action<object, SerializationContext> ForComplex(Type root, Func<Type, TypeDescriptor> descriptorSource)
		{
			var properties = root
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(i => i.GetMethod != null)
				.ToList();

			var fields = root.GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();

			var all = properties
				.Select(i => new {i.Name, Getter = ReflectionUtils.BuildGetter(i), ReturnType = i.PropertyType})
				.Concat(fields.Select(i => new {i.Name, Getter = new Func<object, object>(i.GetValue), ReturnType = i.FieldType}))
				.Select(i => new
				{
					i.Name,
					i.Getter,
					Descriptor = descriptorSource(i.ReturnType)
				});

			return (obj, context) =>
			{
				if (context.Depth <= 0)
					throw new Exception("To deep [todo]");
				var writer = context.Writer;
				if (obj == null)
				{
					writer.WriteNull();
					return;
				}

				writer.BeginObject();
				var it = 0;

				foreach (var item in all)
				{
					if (it++ > 0)
						writer.NextObjectProperty();

					var getter = item.Getter;
					writer.PropertyName(item.Name);
					var value = getter(obj);
					item.Descriptor.Writer(value, context);
				}
				context.Writer.EndObject();
			};
		}
	}
}