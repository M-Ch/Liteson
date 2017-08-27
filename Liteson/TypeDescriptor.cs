using System;

namespace Liteson
{
	internal class TypeDescriptor
	{
		public Type Type { get; set; }
		public Func<DeserializationContext, object> Reader { get; set; }
		public Action<object, SerializationContext> Writer { get; set; }
	}
}