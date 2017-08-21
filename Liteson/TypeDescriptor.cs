using System;

namespace Liteson
{
	internal class TypeDescriptor
	{
		public Type Type { get; set; }
		public SerializationPlan SerializationPlan { get; set; }
		public Func<JsonReader, object> Reader { get; set; }
	}
}