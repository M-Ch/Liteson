using System;
using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Newton = Newtonsoft.Json.JsonConvert;

namespace Liteson.Tests
{
	public class Deserialization
	{
		private readonly ITestOutputHelper _output;

		private class Foo
		{
			public int Value { get; set; }
			public string Text { get; set; }
			public Baz Inner { get; set; }
		}

		private class Baz
		{
			public TimeSpan Time { get; set; }
		}

		public Deserialization(ITestOutputHelper output) => _output = output;

		[Fact]
		public void BasicTypes()
		{
			var data = new BasicTypesBag
			{
				Bool = true,
				Byte = 100,
				Bytes = new byte[] { 1,2,60 },
				Char = '鬱',
				DateTime = DateTime.Parse("2017-08-20 20:34:56"),
				Decimal = 23567890.56m,
				Double = 234.56546342,
				Float = 65.2344f,
				Guid = Guid.NewGuid(),
				Int = 55232,
				Long = 63465463456345634,
				SByte = -120,
				Short = 4231,
				String = "lorem ipsum",
				TimeSpan = TimeSpan.FromMinutes(43),
				UInt = 2323423423,
				ULong = 2345234232345423452,
				UShort = 15340
			};
			var input = Newton.SerializeObject(data);
			JsonConvert.Deserialize<BasicTypesBag>(input).ShouldBeEquivalentTo(data);
		}

		[Fact]
		public void EmptyObject() => JsonConvert.Deserialize<BasicTypesBag>("{}").ShouldBeEquivalentTo(new BasicTypesBag());

		[Fact]
		public void Null() => JsonConvert.Deserialize<BasicTypesBag>("null").ShouldBeEquivalentTo(null);

		[Theory, InlineData("not bool"), InlineData("tru"), InlineData("fals")]
		public void BadBool(string input) => Assert.Throws<JsonException>(() => JsonConvert.Deserialize<bool>($"\"{input}\""));

		[Theory, InlineData("true", true), InlineData("false", false), InlineData("\"true\"", true)]
		[InlineData("\"True\"", true), InlineData("\"False\"", false)]
		[InlineData("-1", true), InlineData("0", false), InlineData("1", true), InlineData("2.5", true), InlineData("0.0000", false)]
		public void Boolean(string input, bool expected) => JsonConvert.Deserialize<bool>(input).ShouldBeEquivalentTo(expected);

		[Fact]
		public void BasicTypesFromStrings()
		{
			const string input =
			@"{
			  ""Bool"":""true"",
			  ""Char"":""鬱"",
			  ""Byte"":""100"",
			  ""SByte"":""-120"",
			  ""Short"":""4231"",
			  ""UShort"":""15340"",
			  ""Int"":""55232"",
			  ""UInt"":""2323423423"",
			  ""Long"":""63465463456345634"",
			  ""ULong"":""2345234232345423452"",
			  ""Double"":""234.56546342"",
			  ""Float"":""65.2344"",
			  ""Decimal"":""23567890.56"",
			  ""Guid"":""f0c7582f-f014-4e21-9261-c015dbf3c19f"",
			  ""TimeSpan"":""00:43:00"",
			  ""DateTime"":""2017-08-20T20:34:56"",
			  ""Bytes"":""AQI8"",
			  ""String"":""lorem ipsum""
			}";
			var expected = new BasicTypesBag
			{
				Bool = true,
				Byte = 100,
				Bytes = new byte[] { 1,2,60 },
				Char = '鬱',
				DateTime = DateTime.Parse("2017-08-20 20:34:56"),
				Decimal = 23567890.56m,
				Double = 234.56546342,
				Float = 65.2344f,
				Guid = Guid.Parse("f0c7582f-f014-4e21-9261-c015dbf3c19f"),
				Int = 55232,
				Long = 63465463456345634,
				SByte = -120,
				Short = 4231,
				String = "lorem ipsum",
				TimeSpan = TimeSpan.FromMinutes(43),
				UInt = 2323423423,
				ULong = 2345234232345423452,
				UShort = 15340
			};
			JsonConvert.Deserialize<BasicTypesBag>(input).ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void ComplexType()
		{
			var expected = new Foo
			{
				Value = 345,
				Text = "lorem ipsum",
				Inner = new Baz
				{
					Time = TimeSpan.FromHours(2.5)
				}
			};
			var input = Newton.SerializeObject(expected);
			JsonConvert.Deserialize<Foo>(input).ShouldBeEquivalentTo(expected);
		}

		//[Fact] run in release mode
		public void Performance()
		{
			var expected = new Foo
			{
				Value = 345,
				Text = "lorem ipsum",
				Inner = new Baz
				{
					Time = TimeSpan.FromHours(2.5)
				}
			};
			var input = Newtonsoft.Json.JsonConvert.SerializeObject(expected);

			var sw = new Stopwatch();
			const int it = 500000;
			sw.Start();
			for(var a = 0; a < it; a++)
				Newtonsoft.Json.JsonConvert.DeserializeObject<Foo>(input);
			var e1 = sw.Elapsed;
			sw.Restart();
			for(var a = 0; a < it; a++)
				JsonConvert.Deserialize<Foo>(input);
			var e2 = sw.Elapsed;
			_output.WriteLine($"Newton: {e1}");
			_output.WriteLine($" Local: {e2}");
		}
	}
}