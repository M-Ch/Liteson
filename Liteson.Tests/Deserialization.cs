using System;
using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

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
			var input = Newtonsoft.Json.JsonConvert.SerializeObject(expected);

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