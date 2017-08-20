using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using Newton = Newtonsoft.Json.JsonConvert;

namespace Liteson.Tests.JsonWriter
{
	public class Primitives
	{
		private readonly StringWriter _sw;
		private readonly Liteson.JsonWriter _writer;

		public Primitives()
		{
			_sw = new StringWriter();
			_writer = new Liteson.JsonWriter(_sw, new WriterSettings());
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
			_sw.ToString().ShouldBeEquivalentTo(Newton.SerializeObject(input));
		}

		[Fact]
		public void ByteArray()
		{
			var data = Encoding.UTF8.GetBytes("sample data for binary serialization.");
			_writer.Write(data);
			_sw.ToString().ShouldBeEquivalentTo(Newton.SerializeObject(data));
		}

		[Fact] public void Char() => Run(() => _writer.Write('c')).ShouldBeEquivalentTo(Newton.SerializeObject('c'));
		[Fact] public void Byte() => Run(() => _writer.Write((byte) 123)).ShouldBeEquivalentTo(Newton.SerializeObject((byte) 123));
		[Fact] public void Sbyte() => Run(() => _writer.Write((sbyte) -30)).ShouldBeEquivalentTo(Newton.SerializeObject((sbyte) -30));
		[Fact] public void Short() => Run(() => _writer.Write((short) -4567)).ShouldBeEquivalentTo(Newton.SerializeObject((short) -4567));
		[Fact] public void UShort() => Run(() => _writer.Write((ushort) 24567)).ShouldBeEquivalentTo(Newton.SerializeObject((ushort)24567));
		[Fact] public void Int() => Run(() => _writer.Write(-6624567)).ShouldBeEquivalentTo(Newton.SerializeObject(-6624567));
		[Fact] public void UInt() => Run(() => _writer.Write(3452345333)).ShouldBeEquivalentTo(Newton.SerializeObject(3452345333));
		[Fact] public void Long() => Run(() => _writer.Write(-6526343232)).ShouldBeEquivalentTo(Newton.SerializeObject(-6526343232));
		[Fact] public void ULong() => Run(() => _writer.Write(9652654543423343232)).ShouldBeEquivalentTo(Newton.SerializeObject(9652654543423343232));
		[Fact] public void Float() => Run(() => _writer.Write(5434.65f)).ShouldBeEquivalentTo(Newton.SerializeObject(5434.65f));
		[Fact] public void Double() => Run(() => _writer.Write(5123434.65)).ShouldBeEquivalentTo(Newton.SerializeObject(5123434.65));
		[Fact] public void Decimal() => Run(() => _writer.Write(3452345354.65344m)).ShouldBeEquivalentTo(Newton.SerializeObject(3452345354.65344m));
		[Fact] public void Guid() => Run(() => _writer.Write(new Guid())).ShouldBeEquivalentTo(Newton.SerializeObject(new Guid()));
		[Fact] public void Bool() => Run(() => _writer.Write(false)).ShouldBeEquivalentTo(Newton.SerializeObject(false));
		[Fact] public void Date() => Run(() => _writer.Write(DateTime.Parse("2012-12-22 14:56:33.78"))).ShouldBeEquivalentTo(Newton.SerializeObject(DateTime.Parse("2012-12-22 14:56:33.78")));
		[Fact] public void Time() => Run(() => _writer.Write(TimeSpan.FromDays(2.345))).ShouldBeEquivalentTo(Newton.SerializeObject(TimeSpan.FromDays(2.345)));

		[Fact] public void UtcDate()
		{
			var date = DateTime.Parse("2013-10-20 13:21:56.678").ToUniversalTime();
			Run(() => _writer.Write(date)).ShouldBeEquivalentTo(Newton.SerializeObject(date));
		}

		[Fact]
		public void LocalDate()
		{
			var date = DateTime.Parse("2013-10-20 13:21:56.678").ToLocalTime();
			Run(() => _writer.Write(date)).ShouldBeEquivalentTo(Newton.SerializeObject(date));
		}


		private string Run(Action action)
		{
			action();
			return _sw.ToString();
		}
	}
}