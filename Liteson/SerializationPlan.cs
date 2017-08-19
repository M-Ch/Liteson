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

		private static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
		private static readonly Type EnumerableType = typeof(IEnumerable);

		public static SerializationPlan ForType(Type type, Func<Type, TypeDescriptor> descriptorSource) => new SerializationPlan
		{
			Steps = EnumerableType.IsAssignableFrom(type)
				? new[] { ForCollection(type, descriptorSource) }
				: ForComplex(type, descriptorSource).ToArray()
		};

		private static Action<object, SerializationContext> ForCollection(Type root, Func<Type, TypeDescriptor> descriptorSource)
		{
			var interfaces = root.GetInterfaces();
			var genericEnumerable = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == GenericEnumerableType);

			var elementType = genericEnumerable?.GetGenericArguments()[0] ?? typeof(object);
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
				.GetTypeInfo()
				.GetRuntimeProperties()
				.Where(i => i.GetMethod != null)
				.ToList();

			yield return (o, context) =>
			{
				if(context.Depth <= 0)
					throw new Exception("To deep [todo]");
				context.Writer.BeginObject();
			};

			var it = 0;
			foreach(var property in properties)
			{
				var subDescriptor = descriptorSource(property.PropertyType);
				if (it++ > 0)
					yield return (obj, context) => context.Writer.NextObjectProperty();

				var getter = ReflectionUtils.BuildGetter(property);
				yield return (obj, context) =>
				{
					var writer = context.Writer;
					writer.PropertyName(property.Name);
					var value = getter(obj);
					ObjectWriter.Write(value, context, subDescriptor.SerializationPlan);
				};
			}

			yield return (o, context) => context.Writer.EndObject();
		}
	}
}