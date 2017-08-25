namespace Liteson
{
	public class SerializationSettings
	{
		public bool CamelCaseNames { get; set; }
		public bool Indent { get; set; }
		public string NewLine { get; set; } = "\r\n";
		public string Tab { get; set; } = "\t";
		public bool EnumsToStrings { get; set; }
	}
}