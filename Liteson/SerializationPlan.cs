using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Liteson
{
	internal class SerializationPlan
	{
		public IReadOnlyList<Action<object, SerializationContext>> Steps { get; set; }
		private static readonly TypeInfo EnumerableType = typeof(IEnumerable).GetTypeInfo();

		public static SerializationPlan ForType(Type type, Func<Type, TypeDescriptor> descriptorSource) => new SerializationPlan
		{
			Steps = EnumerableType.IsAssignableFrom(type.GetTypeInfo())
				? new[] { ForCollection(type, descriptorSource) }
				: ForComplex(type, descriptorSource).ToArray()
		};

		private static Action<object, SerializationContext> ForCollection(Type root, Func<Type, TypeDescriptor> descriptorSource)
		{
			var elementType = ReflectionUtils.FindCollectionElementType(root);
			var descriptor = descriptorSource(elementType);

			return (obj, context) =>
			{
				var writer = context.Writer;
				writer.BeginArray();
				foreach(var item in (IEnumerable)obj)
				{
					writer.ArrayItem();
					ObjectWriter.Write(item, context, descriptor.SerializationPlan);
				}
				writer.EndArray();
			};
		}

		private static IEnumerable<Action<object, SerializationContext>> ForComplex(Type root, Func<Type, TypeDescriptor> descriptorSource)
		{
			var properties = root
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(i => i.GetMethod != null)
				.ToList();

			var fields = root.GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();


			yield return (o, context) =>
			{
				if(context.Depth <= 0)
					throw new Exception("To deep [todo]");
				context.Writer.BeginObject();
			};

			var it = 0;
			var all = properties
				.Select(i => new {i.Name, Getter = ReflectionUtils.BuildGetter(i), ReturnType = i.PropertyType})
				.Concat(fields.Select(i => new {i.Name, Getter = new Func<object, object>(i.GetValue), ReturnType = i.FieldType}));

			foreach(var item in all)
			{
				var subDescriptor = descriptorSource(item.ReturnType);
				if (it++ > 0)
					yield return (obj, context) => context.Writer.NextObjectProperty();

				var getter = item.Getter;
				yield return (obj, context) =>
				{
					var writer = context.Writer;
					writer.PropertyName(item.Name);
					var value = getter(obj);
					ObjectWriter.Write(value, context, subDescriptor.SerializationPlan);
				};
			}


			yield return (o, context) => context.Writer.EndObject();
		}
	}
}