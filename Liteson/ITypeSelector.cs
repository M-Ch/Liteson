using System;

namespace Liteson
{
	public interface ITypeSelector
	{
		Type SupportedType { get; }
		Type FindPropertyType(string property, object parent);
	}
}