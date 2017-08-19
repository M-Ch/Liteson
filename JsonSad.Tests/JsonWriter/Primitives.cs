using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace JsonSad.Tests.JsonWriter
{
	public class Primitives
	{
		private readonly StringWriter _sw;
		private readonly JsonSad.JsonWriter _writer;

		public Primitives()
		{
			_sw = new StringWriter();
			_writer = new JsonSad.JsonWriter(_sw, new WriterSettings());
		}

		[Theory]
		[InlineData("lorem ipusm")]
		[InlineData("http://example.com")]
		[InlineData("two\nlines")]
		[InlineData("line\r\n\tindent")]
		[InlineData("unicode 漢字")]
		public void Strings(string input)
		{
			_writer.Write(input);
			_sw.ToString().ShouldBeEquivalentTo(JsonConvert.SerializeObject(input));
		}
	}
}