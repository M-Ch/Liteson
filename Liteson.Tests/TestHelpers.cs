namespace Liteson.Tests
{
	internal static class TestHelpers
	{
		public static (JsonToken token, string buffer) Read(this JsonReader reader)
		{
			var part = new BufferPart();
			var result = reader.Read(ref part, out var buffer);

			return (result, part.Length > 0 ? part.Text.Substring(part.Start, part.Length) : buffer);
		}
	}
}