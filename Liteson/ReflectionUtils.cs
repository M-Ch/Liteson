using System;
using System.Reflection;

namespace JsonSad
{
	internal static class ReflectionUtils
	{
		private static readonly MethodInfo BuildDelegateMethod = typeof(ReflectionUtils)
			.GetMethod(nameof(BuildDelegate), BindingFlags.NonPublic | BindingFlags.Static);

		public static Func<object, object> BuildGetter(PropertyInfo property)
		{
			var genericBuild = BuildDelegateMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
			return (Func<object, object>) genericBuild.Invoke(null, new object[] {property.GetMethod});
		}

		private static Func<object, object> BuildDelegate<TTarget, TResult>(MethodInfo method) where TTarget : class
		{
			var bound = (Func<TTarget, TResult>)Delegate.CreateDelegate(typeof(Func<TTarget, TResult>), method);
			return i => bound((TTarget) i);
		}
	}
}