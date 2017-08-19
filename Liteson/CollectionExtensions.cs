using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Liteson
{
	internal static class CollectionExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue GetIfPresent<TKey, TValue>(this Dictionary<TKey, TValue> values, TKey key) => values.TryGetValue(key, out var value) ? value : default(TValue);
	}
}