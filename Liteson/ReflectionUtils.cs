using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Liteson
{
	internal static class ReflectionUtils
	{
		private static readonly MethodInfo ParameterlessFuncInfo = typeof(ReflectionUtils)
			.GetTypeInfo()
			.GetDeclaredMethod(nameof(BuildParameterlessFunc));

		private static readonly MethodInfo ParametrizedActionInfo = typeof(ReflectionUtils)
			.GetTypeInfo()
			.GetDeclaredMethod(nameof(BuildParametrizedAction));

		private static readonly MethodInfo BuildConstructorInfo = typeof(ReflectionUtils)
			.GetTypeInfo()
			.GetDeclaredMethod(nameof(BuildConstructorFunc));

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
			var genericBuild = ParametrizedActionInfo.MakeGenericMethod(property.DeclaringType, property.PropertyType);
			return (Action<object, object>)genericBuild.Invoke(null, new object[] { property.SetMethod });
		}

		public static Func<object, object> BuildParameterlessFunc<TTarget, TResult>(MethodInfo method) where TTarget : class
		{
			var bound = (Func<TTarget, TResult>)method.CreateDelegate(typeof(Func<TTarget, TResult>));
			return i => bound((TTarget) i);
		}

		public static Action<object, object> BuildParametrizedAction<TTarget, TParam>(MethodInfo method) where TTarget : class
		{
			var bound = (Action<TTarget, TParam>)method.CreateDelegate(typeof(Action<TTarget, TParam>));
			return (i,p) => bound((TTarget)i, (TParam)p);
		}

		public static Func<object> BuildConstructorFunc<T>()
		{
			var type = typeof(T);
			var method = new DynamicMethod("$CtorCall" + type.Name, type, null, type);
			var generator = method.GetILGenerator();
			generator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Ret);
			var func = (Func<T>)method.CreateDelegate(typeof(Func<T>));
			return () => func();
		}
	}
}