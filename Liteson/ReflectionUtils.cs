using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Liteson
{
	internal static class ReflectionUtils
	{
		private static MethodInfo GetMethod(string name) => typeof(ReflectionUtils)
			.GetTypeInfo()
			.GetDeclaredMethod(name);

		private static readonly MethodInfo ParameterlessFuncInfo = GetMethod(nameof(BuildParameterlessFunc));
		private static readonly MethodInfo ParametrizedActionClassInfo = GetMethod(nameof(BuildParametrizedActionClass));
		private static readonly MethodInfo ParametrizedActionStructInfo = GetMethod(nameof(BuildParametrizedActionStruct));
		private static readonly MethodInfo BuildConstructorInfo = GetMethod(nameof(BuildConstructorFunc));
		private static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
		private static readonly Type ObjectType = typeof(object);

		private delegate TResult RefFunc<TTarget, TResult>(ref TTarget target);
		private delegate void RefAction<TTarget, TParam>(ref TTarget target, TParam parameter);

		public static Func<object> BuildConstructor(Type type)
		{
			var genericBuild = BuildConstructorInfo.MakeGenericMethod(type);
			return (Func<object>)genericBuild.Invoke(null, Array.Empty<object>());
		}

		public static Type FindCollectionElementType(Type enumerableType)
		{
			var interfaces = enumerableType.GetTypeInfo().ImplementedInterfaces.Select(i => i.GetTypeInfo()).ToList();
			var genericEnumerable = new[] { enumerableType.GetTypeInfo() }.Concat(interfaces).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == GenericEnumerableType);

			return genericEnumerable?.GenericTypeArguments[0] ?? ObjectType;
		}

		public static Func<object, object> BuildGetter(PropertyInfo property)
		{
			var genericBuild = ParameterlessFuncInfo.MakeGenericMethod(property.DeclaringType, property.PropertyType);
			return (Func<object, object>) genericBuild.Invoke(null, new object[] {property.GetMethod});
		}

		public static Action<object, object> BuildSetter(PropertyInfo property)
		{
			var method = property.DeclaringType.GetTypeInfo().IsClass
				? ParametrizedActionClassInfo
				: ParametrizedActionStructInfo;
			var genericBuild = method.MakeGenericMethod(property.DeclaringType, property.PropertyType);
			return (Action<object, object>)genericBuild.Invoke(null, new object[] { property.SetMethod });
		}

		public static Func<object, object> BuildParameterlessFunc<TTarget, TResult>(MethodInfo method)
		{
			if (typeof(TTarget).GetTypeInfo().IsClass)
			{
				var bound = (Func<TTarget, TResult>) method.CreateDelegate(typeof(Func<TTarget, TResult>));
				return i => bound((TTarget) i);
			}
			else
			{
				var bound = (RefFunc<TTarget, TResult>) method.CreateDelegate(typeof(RefFunc<TTarget, TResult>));
				return i =>
				{
					var unboxed = (TTarget)i;
					return bound(ref unboxed);
				};
			}
		}

		public static Action<object, object> BuildParametrizedActionClass<TTarget, TParam>(MethodInfo method) where TTarget : class
		{
			var bound = (Action<TTarget, TParam>) method.CreateDelegate(typeof(Action<TTarget, TParam>));
			return (i, p) => bound((TTarget) i, (TParam) p);
		}

		public static Action<object, object> BuildParametrizedActionStruct<TTarget, TParam>(MethodInfo method) where TTarget : struct
		{
			var bound = (RefAction<TTarget, TParam>) method.CreateDelegate(typeof(RefAction<TTarget, TParam>));
			return (i, p) =>
			{
				var box = (Box<TTarget>)i;
				bound(ref box.Value, (TParam)p);
				box.InvalidateWrapper();
			};
		}

		public static Action<object, object> BuildFieldSetter(FieldInfo field)
		{
			if (field.DeclaringType.GetTypeInfo().IsClass)
				return field.SetValue;

			return (target, value) =>
			{
				var box = (IBox) target;
				field.SetValue(box.Value, value);
			};
		}

		public static Func<object> BuildConstructorFunc<T>()
		{
			var type = typeof(T);
			var info = type.GetTypeInfo();
			var constructor = info.GetConstructor(Type.EmptyTypes);
			var newExp = Expression.New(constructor);
			var func = (Func<T>)Expression.Lambda(typeof(Func<T>), newExp).Compile();
			return () => func();
		}
	}

	internal interface IBox
	{
		object Value { get; }
	}

	internal class Box<T> : IBox where T : struct
	{
		public T Value;
		private object _box;

		public void InvalidateWrapper() => _box = null;
		object IBox.Value => _box ?? (_box = Value);
	}
}