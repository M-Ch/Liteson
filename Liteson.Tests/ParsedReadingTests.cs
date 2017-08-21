using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Liteson.Tests
{
	public class ParsedReadingTests
	{
		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void Ints(string input, int expected) => ParsedReading.ReadInt(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void Bytes(string input, byte expected) => ParsedReading.ReadByte(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void SBytes(string input, sbyte expected) => ParsedReading.ReadSByte(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void Shorts(string input, short expected) => ParsedReading.ReadShort(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void UShorts(string input, ushort expected) => ParsedReading.ReadUShort(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void UInts(string input, uint expected) => ParsedReading.ReadUInt(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void Longs(string input, long expected) => ParsedReading.ReadLong(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1", 1), InlineData("\"10\"", 10)]
		public void ULongs(string input, ulong expected) => ParsedReading.ReadULong(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1.1", 1.1f), InlineData("\"10.1\"", 10.1f)]
		public void Floats(string input, float expected) => ParsedReading.ReadFloat(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory, InlineData("1.1", 1.1), InlineData("\"10.1\"", 10.1)]
		public void Doubles(string input, double expected) => ParsedReading.ReadDouble(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		[Theory]
		[MemberData(nameof(DecimalsSource))]
		public void Decimals(string input, decimal expected) => ParsedReading.ReadDecimal(new JsonReader(input)).ShouldBeEquivalentTo(expected);

		public static IEnumerable<object[]> DecimalsSource()
		{
			yield return new object[] {"1", 1m};
			yield return new object[] {"1.0", 1.0m};
			yield return new object[] {"1.00000000", 1.00000000m };
		}

		[Theory]
		[InlineData("1e2")]
		[InlineData("1.5")]
		[InlineData("1.0")]
		[InlineData("\"1e2\"")]
		public void BadInts(string input) => Assert.Throws<JsonException>(() => ParsedReading.ReadInt(new JsonReader(input)));

		[Fact] public void BadByte() => Assert.Throws<JsonException>(() => ParsedReading.ReadByte(new JsonReader("-2")));
		[Fact] public void BadSByte() => Assert.Throws<JsonException>(() => ParsedReading.ReadByte(new JsonReader("156783")));
	}
}