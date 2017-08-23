using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Liteson.Tests
{
	public class ParsingTests
	{
		private readonly ITestOutputHelper _output;
		public ParsingTests(ITestOutputHelper output) => _output = output;

		[Theory]
		[InlineData("0", 0)]
		[InlineData("-0", 0)]
		[InlineData("1", 1)]
		[InlineData("-1", -1)]
		[InlineData("23", 23)]
		[InlineData("-23", -23)]
		[InlineData("5623452", 5623452)]
		[InlineData("2147483647", 2147483647)]
		[InlineData("-2147483648", -2147483648)]
		public void Int(string input, int expected)
		{
			Parsing.TryParseBase10Int(input, 0, input.Length, out var result).Should().BeTrue();
			result.ShouldBeEquivalentTo(expected);
		}

		[Theory]
		[InlineData("2147483648")]
		[InlineData("11474836411")]
		[InlineData("9999999999")]
		[InlineData("2999999999")]
		[InlineData("-9999999999")]
		[InlineData("-2999999999")]
		[InlineData("-2147483649")]
		[InlineData("-11474836490")]
		public void Overflows(string input) => Parsing.TryParseBase10Int(input, 0, input.Length, out var _).Should().BeFalse();

		//[Fact]
		public void Performance()
		{
			var sw = new Stopwatch();
			const int iterations = 5000000;
			const string value = "5678";
			sw.Start();
			for (var a = 0; a < iterations; a++)
				int.TryParse(value, out var _);
			var elapsed = sw.Elapsed;
			sw.Restart();
			for(var a = 0; a < iterations; a++)
				Parsing.TryParseBase10Int(value, 0, value.Length, out var _);
			var elapsed2 = sw.Elapsed;

			_output.WriteLine($" .Net: {elapsed}");
			_output.WriteLine($"Local: {elapsed2}");
		}
	}
}