using System.Diagnostics;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Liteson.Tests
{
	public class FormattingTests
	{
		private readonly ITestOutputHelper _output;
		public FormattingTests(ITestOutputHelper output) => _output = output;

		[Theory]
		[InlineData(0), InlineData(1), InlineData(int.MaxValue), InlineData(int.MinValue), InlineData(10), InlineData(100), InlineData(2532)]
		[InlineData(-20546), InlineData(-1), InlineData(-10), InlineData(-100), InlineData(-2532), InlineData(10005)]
		public void Ints(int value)
		{
			var sw = new StringWriter();
			Formatting.WriteFast(value, sw, new byte[20]);
			sw.ToString().ShouldBeEquivalentTo(value.ToString());
		}

		//[Fact] //run in release mode
		public void Performance()
		{
			var sw = new Stopwatch();
			const int iterations = 10000000;
			sw.Start();
			var sb = new StringBuilder();
			var writer = new StringWriter(sb);
			var buffer = new byte[20];
			for (var a = 0; a < iterations; a++)
				57234678.ToString();
			var elapsed = sw.Elapsed;
			sw.Restart();
			for (var a = 0; a < iterations; a++)
			{
				Formatting.WriteFast(57234678, writer, buffer);
				sb.Clear();
			}
			var elapsed2 = sw.Elapsed;

			_output.WriteLine($" .Net: {elapsed}");
			_output.WriteLine($"Local: {elapsed2}");
		}
	}
}