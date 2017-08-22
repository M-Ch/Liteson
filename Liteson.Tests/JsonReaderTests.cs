using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Liteson.Tests
{
	public class JsonReaderTests
	{
		[Theory]
		[InlineData("123", JsonToken.Number)]
		[InlineData("-123", JsonToken.Number | JsonToken.NumberNegative)]
		[InlineData("123.1", JsonToken.Number | JsonToken.NumberFloat)]
		[InlineData("-123.1", JsonToken.Number | JsonToken.NumberFloat | JsonToken.NumberNegative)]
		[InlineData("123e0", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("123e1", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("123e+0", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("123e-0", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("123e-03", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("123e-3", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("123e+00003", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("1e+00003", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("15345355e+00003", JsonToken.Number | JsonToken.NumberExponent)]
		[InlineData("15345.355e+00003", JsonToken.Number | JsonToken.NumberExponent | JsonToken.NumberFloat)]
		[InlineData("-15345.355e+00003", JsonToken.Number | JsonToken.NumberExponent | JsonToken.NumberFloat | JsonToken.NumberNegative)]
		public void Numbers(string input, int expected) => ReadTokens(input).ShouldBeEquivalentTo(new[] {((JsonToken) expected, input)});

		[Theory]
		[InlineData("null", JsonToken.Null)]
		[InlineData("true", JsonToken.True)]
		[InlineData("false", JsonToken.False)]
		[InlineData("null     ", JsonToken.Null)]
		[InlineData("     null     ", JsonToken.Null)]
		public void Primitives(string input, int expected) 
			=> ReadTokens(input).ShouldBeEquivalentTo(new[] { ((JsonToken) expected, (string)null) });

		[Theory]
		[InlineData("[]", new[] { (int)JsonToken.ArrayStart, (int)JsonToken.ArrayEnd })]
		[InlineData("[ true     ]     ", new[] { (int)JsonToken.ArrayStart, (int) JsonToken.True, (int)JsonToken.ArrayEnd })]
		[InlineData("[ true,null     ]     ", new[] { (int)JsonToken.ArrayStart, (int) JsonToken.True, (int)JsonToken.ValueSeparator, (int)JsonToken.Null, (int)JsonToken.ArrayEnd })]
		public void TokenSequences(string input, int[] expected)
			=> ReadTokens(input).ShouldBeEquivalentTo(expected.Select(i => ((JsonToken) i, (string) null)));

		[Fact]
		public void SimpleObject() => ReadTokens(@"{""value"": 12}").ShouldBeEquivalentTo(new[]
		{
			(JsonToken.ObjectStart, (string)null),
			(JsonToken.String, "value"),
			(JsonToken.NameSeparator, null),
			(JsonToken.Number, "12"),
			(JsonToken.ObjectEnd, null)
		});

		[Theory]
		[InlineData("abc", "abc")]
		[InlineData("a b", "a b")]
		[InlineData("漢字", "漢字")]
		[InlineData("", "")]
		[InlineData("escaped \\u3060\\u3081", "escaped だめ")]
		public void Strings(string input, string expected) => ReadTokens($"\"{input}\"").ShouldBeEquivalentTo(new[] { (JsonToken.String, expected) });

		[Theory]
		[InlineData("1.")]
		[InlineData("3.1f")]
		[InlineData("3ez")]
		[InlineData("True")]
		[InlineData("False")]
		[InlineData("Null")]
		[InlineData("--")]
		[InlineData("+1")]
		[InlineData("foo")]
		[InlineData("fals")]
		[InlineData("truE")]
		[InlineData("<")]
		public void InvalidTokens(string input)
		{
			var reader = new JsonReader(input);
			Assert.Throws<JsonException>(() => reader.Read());
		}

		[Theory]
		[InlineData("a\\gc")]
		[InlineData("a\\U1212")]
		[InlineData("a\\u1g12")]
		[InlineData("a\\uh112")]
		[InlineData("a\\u11h2")]
		[InlineData("a\\u111m")]
		public void InvalidStrings(string input)
		{
			var reader = new JsonReader($"\"{input}\"");
			Assert.Throws<JsonException>(() => reader.Read());
		}

		private static IEnumerable<(JsonToken token, string buffer)> ReadTokens(string input)
		{
			var reader = new JsonReader(input);
			while (!reader.IsAtEnd())
			{
				var result = reader.Read();
				if(result.token != JsonToken.End)
					yield return result;
			}
		}
	}
}