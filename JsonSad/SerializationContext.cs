namespace JsonSad
{
	internal class SerializationContext
	{
		public int Depth { get; set; }
		public JsonWriter Writer { get; set; }
	}
}