using System;
using System.Text;
using FluentAssertions;
using Xunit;

namespace JsonSad.Tests
{
	public class Base64EncodingTests
	{
		[Fact]
		public void SimpleBlock()
		{
			var data = new byte[] {1, 2, 3};
			Base64.Write(data).ShouldBeEquivalentTo(Convert.ToBase64String(data));
		}

		[Fact]
		public void Zeros()
		{
			var data = new byte[] {0,0,0};
			Base64.Write(data).ShouldBeEquivalentTo(Convert.ToBase64String(data));
		}

		[Fact]
		public void EmptyArray() => Base64.Write(Array.Empty<byte>()).ShouldBeEquivalentTo(Convert.ToBase64String(Array.Empty<byte>()));

		[Fact]
		public void Null() => Assert.Throws<ArgumentNullException>(() => Base64.Write(null));

		[Fact]
		public void IncompleteBlockOneByte()
		{
			var data = new byte[] {123, 98, 200, 45};
			Base64.Write(data).ShouldBeEquivalentTo(Convert.ToBase64String(data));
		}

		[Fact]
		public void IncompleteBlockTwoBytes()
		{
			var data = new byte[] {15, 243, 122, 99, 87};
			Base64.Write(data).ShouldBeEquivalentTo(Convert.ToBase64String(data));
		}

		[Fact]
		public void LoremIpsum()
		{
			var data = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras id purus ultrices diam elementum venenatis convallis vitae erat. Fusce facilisis quam in fermentum posuere. Cras in ornare eros, ut sollicitudin sem. Aenean nec faucibus augue. Curabitur viverra ante nec ipsum mollis, non ornare urna fringilla. Cras id mollis ipsum, eget ullamcorper risus. Nunc at magna magna. Quisque tempor suscipit interdum. Vivamus tincidunt, mi eu luctus cursus, diam ipsum blandit augue, ut tincidunt urna dui eu magna. Curabitur nunc lorem, viverra nec nisl in, facilisis auctor nibh. Curabitur feugiat volutpat tempor. Nam facilisis consequat aliquet. ");

			Base64.Write(data).ShouldBeEquivalentTo(Convert.ToBase64String(data));
		}
	}
}