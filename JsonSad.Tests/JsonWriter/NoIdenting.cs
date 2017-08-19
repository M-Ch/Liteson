using System.IO;
using FluentAssertions;
using Xunit;

namespace JsonSad.Tests.JsonWriter
{
	public class NoIdenting
	{
		private readonly StringWriter _sw;
		private readonly JsonSad.JsonWriter _writer;

		public NoIdenting()
		{
			_sw = new StringWriter();
			_writer = new JsonSad.JsonWriter(_sw, new WriterSettings());
		}

		[Fact]
		public void Object()
		{
			_writer.BeginObject();
			_writer.PropertyName("foo");
			_writer.Write(1);
			_writer.EndObject();

			_sw.ToString().ShouldBeEquivalentTo("{\"foo\":1}");
		}
	}
}