using System;
using System.Collections.Generic;
using System.Linq;

namespace Liteson
{
	public class SerializationSettings
	{
		public bool CamelCaseNames { get; set; }
		public bool Indent { get; set; }
		public string NewLine { get; set; } = "\r\n";
		public string Tab { get; set; } = "\t";
		public int MaxDepth { get; set; } = 20;
		public bool EnumsToStrings { get; set; }
		internal Dictionary<Type, ITypeSelector> TypeSelectors { get; } = new Dictionary<Type, ITypeSelector>();

		public SerializationSettings AddTypeSelector(ITypeSelector selector)
		{
			if(TypeSelectors.TryGetValue(selector.SupportedType, out var existing))
			{
				if(existing is CombinedSelector combined)
					combined.AddSelector(existing);
				else
					TypeSelectors[selector.SupportedType] = new CombinedSelector(selector.SupportedType)
						.AddSelector(existing)
						.AddSelector(selector);
			}
			else
				TypeSelectors[selector.SupportedType] = selector;
			return this;
		}

		internal class CombinedSelector : ITypeSelector
		{
			private readonly List<ITypeSelector> _selectors = new List<ITypeSelector>();
			public Type SupportedType { get; }
			public CombinedSelector(Type supportedType) => SupportedType = supportedType;

			public CombinedSelector AddSelector(ITypeSelector selector)
			{
				if(selector.SupportedType != SupportedType)
					throw new ArgumentException(nameof(selector));
				_selectors.Add(selector);
				return this;
			}

			public Type FindPropertyType(string property, object parent) => _selectors.Select(i => FindPropertyType(property, parent)).FirstOrDefault(i => i != null);
		}
	}
}