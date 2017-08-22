using FluentAssertions;
using Xunit;

namespace Liteson.Tests
{
	public class ValueSkipperTests
	{
		[Theory]
		[InlineData("[]"), InlineData("[{}]"), InlineData("[{\"foo\": 1}]"), InlineData("[1,[1,[1,[1,[1,[1,2,3,4],2],3],4],{\"a\":1}],6]")]
		[InlineData("{\"foo\": [1,2,{\"bar\" : [] }] } "), InlineData("true"), InlineData("123"), InlineData("     123")]
		[InlineData("false"), InlineData("1"), InlineData("2.45e-2"), InlineData("-23"), InlineData("-2.4"), InlineData("-2E10"), InlineData("-2.5e4")]
		public void ValidSequence(string input) => SkipFirst($"{input} null").ShouldBeEquivalentTo(JsonToken.Null);

		[Theory]
		[InlineData("["), InlineData("]"), InlineData("[}"), InlineData("}{"), InlineData("[[")]
		[InlineData("[00]"), InlineData("{\"foo\",\"bar\"}"), InlineData("[1,2"), InlineData("[1,]")]
		[InlineData("[[[{]}]]"), InlineData(",]"), InlineData(":1")]
		public void InvalidSequence(string input) => Assert.Throws<JsonException>(() => ValueSkipper.SkipNext(new JsonReader(input)));

		private static JsonToken SkipFirst(string input)
		{
			var part = new BufferPart();
			var reader = new JsonReader(input);
			ValueSkipper.SkipNext(reader);
			return reader.Read(ref part, out var _);
		}
	}
}