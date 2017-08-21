using System;
using FluentAssertions;
using Xunit;

namespace Liteson.Tests
{
	public class Deserialization
	{
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
	}
}