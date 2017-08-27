using System;
using System.Collections.Generic;

namespace Liteson
{
	public class SerializationSettings
	{
		public bool CamelCaseNames { get; set; }
		public bool Indent { get; set; }
		public string NewLine { get; set; } = "\r\n";
		public string Tab { get; set; } = "\t";
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
	}
}