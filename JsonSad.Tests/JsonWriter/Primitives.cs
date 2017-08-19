using System;
using System.IO;
using System.Text;
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

		[Fact]
		public void ByteArray()
		{
			var data = Encoding.UTF8.GetBytes("sample data for binary serialization.");
			_writer.Write(data);
			_sw.ToString().ShouldBeEquivalentTo(JsonConvert.SerializeObject(data));
		}

		[Fact] public void Char() => Run(() => _writer.Write('c')).ShouldBeEquivalentTo(JsonConvert.SerializeObject('c'));
		[Fact] public void Byte() => Run(() => _writer.Write((byte) 123)).ShouldBeEquivalentTo(JsonConvert.SerializeObject((byte) 123));
		[Fact] public void Sbyte() => Run(() => _writer.Write((sbyte) -30)).ShouldBeEquivalentTo(JsonConvert.SerializeObject((sbyte) -30));
		[Fact] public void Short() => Run(() => _writer.Write((short) -4567)).ShouldBeEquivalentTo(JsonConvert.SerializeObject((short) -4567));
		[Fact] public void UShort() => Run(() => _writer.Write((ushort) 24567)).ShouldBeEquivalentTo(JsonConvert.SerializeObject((ushort)24567));
		[Fact] public void Int() => Run(() => _writer.Write(-6624567)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(-6624567));
		[Fact] public void UInt() => Run(() => _writer.Write(3452345333)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(3452345333));
		[Fact] public void Long() => Run(() => _writer.Write(-6526343232)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(-6526343232));
		[Fact] public void ULong() => Run(() => _writer.Write(9652654543423343232)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(9652654543423343232));
		[Fact] public void Float() => Run(() => _writer.Write(5434.65f)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(5434.65f));
		[Fact] public void Double() => Run(() => _writer.Write(5123434.65)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(5123434.65));
		[Fact] public void Decimal() => Run(() => _writer.Write(3452345354.65344m)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(3452345354.65344m));
		[Fact] public void Guid() => Run(() => _writer.Write(new Guid())).ShouldBeEquivalentTo(JsonConvert.SerializeObject(new Guid()));
		[Fact] public void Bool() => Run(() => _writer.Write(false)).ShouldBeEquivalentTo(JsonConvert.SerializeObject(false));
		[Fact] public void Date() => Run(() => _writer.Write(DateTime.Parse("2012-12-22 14:56:33.78"))).ShouldBeEquivalentTo(JsonConvert.SerializeObject(DateTime.Parse("2012-12-22 14:56:33.78")));
		[Fact] public void Time() => Run(() => _writer.Write(TimeSpan.FromDays(2.345))).ShouldBeEquivalentTo(JsonConvert.SerializeObject(TimeSpan.FromDays(2.345)));

		private string Run(Action action)
		{
			action();
			return _sw.ToString();
		}
	}
}