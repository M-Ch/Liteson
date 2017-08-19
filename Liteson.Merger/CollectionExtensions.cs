using System;
using System.Collections;
using System.Collections.Generic;

namespace Liteson.Merger
{
	public static class CollectionExtensions
	{
		public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> values, Func<T, TKey> keySelector)
		{
			var lookup = new HashSet<TKey>();
			foreach (var value in values)
			{
				var key = keySelector(value);
				if (lookup.Contains(key))
					continue;
				lookup.Add(key);
				yield return value;
			}
		}
	}
}