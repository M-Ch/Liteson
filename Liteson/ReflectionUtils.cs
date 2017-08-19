using System;
using System.Reflection;

namespace Liteson
{
	internal static class ReflectionUtils
	{
		private static readonly MethodInfo BuildDelegateMethod = typeof(ReflectionUtils)
			.GetTypeInfo()
			.GetDeclaredMethod(nameof(BuildDelegate));

		public static Func<object, object> BuildGetter(PropertyInfo property)
		{
			var genericBuild = BuildDelegateMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
			return (Func<object, object>) genericBuild.Invoke(null, new object[] {property.GetMethod});
		}

		public static Func<object, object> BuildDelegate<TTarget, TResult>(MethodInfo method) where TTarget : class
		{
			var bound = (Func<TTarget, TResult>)method.CreateDelegate(typeof(Func<TTarget, TResult>));
			return i => bound((TTarget) i);
		}
	}
}