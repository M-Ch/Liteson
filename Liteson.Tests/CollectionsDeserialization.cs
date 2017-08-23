using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Newton = Newtonsoft.Json.JsonConvert;

namespace Liteson.Tests
{
	public class CollectionsDeserialization
	{
		[Fact] public void Array() => JsonConvert.Deserialize<int[]>("[1,2]").ShouldBeEquivalentTo(new[] {1, 2});
		[Fact] public void List() => JsonConvert.Deserialize<List<int>>("[1,2]").ShouldBeEquivalentTo(new[] {1, 2});
		[Fact] public void IList() => JsonConvert.Deserialize<IList<int>>("[1,2]").ShouldBeEquivalentTo(new[] {1, 2});
		[Fact] public void IEnumerable() => JsonConvert.Deserialize<IEnumerable<int>>("[1,2]").ShouldBeEquivalentTo(new[] {1, 2});
		[Fact] public void ICollection() => JsonConvert.Deserialize<ICollection<int>>("[1,2]").ShouldBeEquivalentTo(new[] {1, 2});
		[Fact] public void IReadOnlyList() => JsonConvert.Deserialize<IReadOnlyList<int>>("[1,2]").ShouldBeEquivalentTo(new[] {1, 2});
		[Fact] public void IReadOnlyCollection() => JsonConvert.Deserialize<IReadOnlyCollection<int>>("[1,2]").ShouldBeEquivalentTo(new[] {1, 2});

		[Fact] public void NullArray() => JsonConvert.Deserialize<int[]>("null").ShouldBeEquivalentTo(null);
		[Fact] public void NullList() => JsonConvert.Deserialize<List<int>>("null").ShouldBeEquivalentTo(null);
		[Fact] public void NullIList() => JsonConvert.Deserialize<IList<int>>("null").ShouldBeEquivalentTo(null);
		[Fact] public void NullIEnumerable() => JsonConvert.Deserialize<IEnumerable<int>>("null").ShouldBeEquivalentTo(null);
		[Fact] public void NullICollection() => JsonConvert.Deserialize<ICollection<int>>("null").ShouldBeEquivalentTo(null);
		[Fact] public void NullIReadOnlyList() => JsonConvert.Deserialize<IReadOnlyList<int>>("null").ShouldBeEquivalentTo(null);
		[Fact] public void NullIReadOnlyCollection() => JsonConvert.Deserialize<IReadOnlyCollection<int>>("null").ShouldBeEquivalentTo(null);

		[Theory]
		[InlineData("["), InlineData("[1,,]"), InlineData("[["), InlineData("{"), InlineData("[,1]"), InlineData("[,,]")]
		[InlineData("true"), InlineData("false"), InlineData("1"), InlineData("{}"), InlineData("\"a\""), InlineData("1.5")]
		public void InvalidCollection(string input) => Assert.Throws<JsonException>(() => JsonConvert.Deserialize<List<int>>(input));

		[Fact]
		public void VeryNestedList() => JsonConvert
			.Deserialize<IEnumerable<ICollection<List<IReadOnlyList<SimplePoco>>>>>("[[[[{},{},{\"IsImportant\": true}]]]]")
			.ShouldBeEquivalentTo(new List<List<List<List<SimplePoco>>>>
			{
				new List<List<List<SimplePoco>>>
				{
					new List<List<SimplePoco>>
					{
						new List<SimplePoco>
						{
							new SimplePoco(),
							new SimplePoco(),
							new SimplePoco { IsImportant = true }
						}
					}
				}
			});


		[Fact]
		public void ObjectWithList()
		{
			var expected = new WithList
			{
				Value = 500,
				Items = new[]
				{
					new SimplePoco {IsImportant = true, Value = "foo"},
					new SimplePoco {Value = "bar"},
					new SimplePoco {IsImportant = true, Value = "baz"},
				}
			};
			JsonConvert.Deserialize<WithList>(Newton.SerializeObject(expected)).ShouldBeEquivalentTo(expected);
		}
	}
}