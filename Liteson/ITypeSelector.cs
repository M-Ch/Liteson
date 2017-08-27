using System;
using System.Collections.Generic;
using System.Linq;

namespace Liteson
{
	public interface ITypeSelector
	{
		Type SupportedType { get; }
		Type FindPropertyType(string property, object parent);
	}

	internal class CombinedSelector : ITypeSelector
	{
		private readonly List<ITypeSelector> _selectors = new List<ITypeSelector>();
		public Type SupportedType { get; }
		public CombinedSelector(Type supportedType) => SupportedType = supportedType;

		public CombinedSelector AddSelector(ITypeSelector selector)
		{
			if (selector.SupportedType != SupportedType)
				throw new ArgumentException(nameof(selector));
			_selectors.Add(selector);
			return this;
		}

		public Type FindPropertyType(string property, object parent) => _selectors.Select(i => FindPropertyType(property, parent)).FirstOrDefault(i => i != null);
	}
}