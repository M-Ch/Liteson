using System;

namespace Liteson
{
	internal class TypeDescriptor
	{
		public Type Type { get; set; }
		public Func<JsonReader, object> Reader { get; set; }
		public Action<object, SerializationContext> Writer { get; set; }
	}
}