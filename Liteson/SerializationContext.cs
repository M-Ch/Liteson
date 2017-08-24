using System.Text;

namespace Liteson
{
	internal class SerializationContext
	{
		public int Depth { get; set; }
		public JsonWriter Writer { get; set; }
		public StringBuilder StringBuilder { get; set; }
	}
}