using System;
using System.Reflection;
using System.Reflection.Emit;

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

		private delegate TResult RefFunc<TTarget, TResult>(ref TTarget target);
		private delegate void RefAction<TTarget, TParam>(ref TTarget target, TParam parameter);

		public static Func<object> BuildConstructor(Type type)
		{
			var genericBuild = BuildConstructorInfo.MakeGenericMethod(type);
			return (Func<object>)genericBuild.Invoke(null, Array.Empty<object>());
		}

		public static Func<object, object> BuildGetter(PropertyInfo property)
		{
			var genericBuild = ParameterlessFuncInfo.MakeGenericMethod(property.DeclaringType, property.PropertyType);
			return (Func<object, object>) genericBuild.Invoke(null, new object[] {property.GetMethod});
		}

		public static Action<object, object> BuildSetter(PropertyInfo property)
		{
			var method = property.DeclaringType.IsClass
				? ParametrizedActionClassInfo
				: ParametrizedActionStructInfo;
			var genericBuild = method.MakeGenericMethod(property.DeclaringType, property.PropertyType);
			return (Action<object, object>)genericBuild.Invoke(null, new object[] { property.SetMethod });
		}

		public static Func<object, object> BuildParameterlessFunc<TTarget, TResult>(MethodInfo method)
		{
			if (typeof(TTarget).IsClass)
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
			};
		}

		public static Func<object> BuildConstructorFunc<T>()
		{
			var type = typeof(T);
			var method = new DynamicMethod("$Ctor" + type.Name + type.GetHashCode(), type, null, type);
			var generator = method.GetILGenerator();
			generator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Ret);
			var func = (Func<T>)method.CreateDelegate(typeof(Func<T>));
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
		object IBox.Value => Value;
	}
}