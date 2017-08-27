using System;

namespace Liteson
{
	internal static class Actions
	{
		public static IDisposable OnDispose(Action action) => new OnDisposeWrapper(action);

		private class OnDisposeWrapper : IDisposable
		{
			private readonly Action _action;
			public OnDisposeWrapper(Action action) => _action = action;
			public void Dispose() => _action();
		}
	}
}