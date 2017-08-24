using System.IO;
using FluentAssertions;
using Xunit;

namespace Liteson.Tests.JsonWriter
{
	public class NoIndenting
	{
		private readonly StringWriter _sw;
		private readonly Liteson.JsonWriter _writer;

		public NoIndenting()
		{
			_sw = new StringWriter();
			_writer = new Liteson.JsonWriter(_sw, new SerializationSettings());
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