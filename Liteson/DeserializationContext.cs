using System;
using System.Collections.Generic;

namespace Liteson
{
	internal class DeserializationContext
	{
		public JsonReader Reader { get; set; }
		public TypeCatalog Catalog { get; set; }
		public IReadOnlyDictionary<Type, ITypeSelector> TypeSelectors { get; set; }
	}
}