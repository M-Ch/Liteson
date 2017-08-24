using System;

namespace Liteson
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class JsonProperty : Attribute
	{
		public string Name { get; }
		public JsonProperty(string name) => Name = name;
	}
}