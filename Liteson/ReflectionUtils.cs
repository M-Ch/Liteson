using System;
using System.Reflection;

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
	}
}