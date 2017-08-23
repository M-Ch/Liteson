using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using Xunit;
using Newton = Newtonsoft.Json.JsonConvert;

namespace Liteson.Tests
{
	public class CollectionsSerialization
	{
		[Fact]
		public void VeryNestedList() => JsonConvert.Serialize(new List<List<List<List<SimplePoco>>>>
		{
			new List<List<List<SimplePoco>>>
			{
				new List<List<SimplePoco>>
				{
					new List<SimplePoco>
					{
						new SimplePoco {IsImportant = true}
					}
				}
			}
		}).ShouldBeEquivalentTo("[[[[{\"IsImportant\":true,\"Value\":null}]]]]");

		[Fact]
		public void ObjectWithList()
		{
			var data = new WithList
			{
				Value = 500,
				Items = new[]
				{
					new SimplePoco {IsImportant = true, Value = "foo"},
					new SimplePoco {Value = "bar"},
					new SimplePoco {IsImportant = true, Value = "baz"},
				}
			};
			JsonConvert.Serialize(data).ShouldBeEquivalentTo(Newton.SerializeObject(data));
		}

		[Fact] public void Array() => JsonConvert.Serialize(new[] { 1, 2 }).ShouldBeEquivalentTo("[1,2]");
		[Fact] public void List() => JsonConvert.Serialize(new List<int> { 1, 2 }).ShouldBeEquivalentTo("[1,2]");
		[Fact] public void HashSet() => JsonConvert.Serialize(new HashSet<int> { 1, 2 }).ShouldBeEquivalentTo("[1,2]");
		[Fact] public void LinkedList() => JsonConvert.Serialize(new LinkedList<int>(new[] { 1, 2 })).ShouldBeEquivalentTo("[1,2]");
		[Fact] public void ObservableCollection() => JsonConvert.Serialize(new ObservableCollection<int>(new[] { 1, 2 })).ShouldBeEquivalentTo("[1,2]");
		[Fact] public void CustomCollection() => JsonConvert.Serialize(new CustomCollectionClass()).ShouldBeEquivalentTo("[1,2]");

		private class CustomCollectionClass : IEnumerable<int>
		{
			public IEnumerator<int> GetEnumerator()
			{
				return Values().GetEnumerator();
				IEnumerable<int> Values()
				{
					yield return 1;
					yield return 2;
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}