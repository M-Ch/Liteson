using System;

namespace Liteson
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class JsonPropertyAttribute : Attribute
	{
		public string Name { get; }
		public JsonPropertyAttribute(string name) => Name = name;
	}
}