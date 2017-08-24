using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Newton = Newtonsoft.Json.JsonConvert;

namespace Liteson.Tests
{
	public class Serialization
	{
		[Fact]
		public void SimpleObject() => JsonConvert.Serialize(new {Foo = 1, Bar = 2}).ShouldBeEquivalentTo("{\"Foo\":1,\"Bar\":2}");

		[Fact]
		public void ComplexObject()
		{
			var data = new[]
			{
				new GameData
				{
					Id = 4,
					Name = "Json Ultimate",
					Notes = "Game of the year",
					Releases = new List<ReleaseInfo>
					{
						new ReleaseInfo
						{
							Date = DateTime.Parse("2017-08-19"),
							Platforms = new[] {"psx", "pc", "xbox one"},
							Region = "eu"
						},
						new ReleaseInfo
						{
							Date = DateTime.Parse("2017-08-19").AddDays(10),
							Platforms = new[] {"ps4"},
							Region = "jp"
						}
					}
				},
				new GameData
				{
					Id = 14,
					Name = "Crash Jsoncoot",
					Releases = new List<ReleaseInfo>
					{
						new ReleaseInfo
						{
							Date = DateTime.Parse("2017-08-19").AddYears(2),
							Region = "eu"
						}
					}
				}
			};

			JsonConvert.Serialize(data).ShouldBeEquivalentTo(Newton.SerializeObject(data));
		}

		[Fact]
		public void BasicTypes()
		{
			var data = new BasicTypesBag
			{
				Bool = true,
				Byte = 100,
				Bytes = new byte[] { 1,2,60 },
				Char = '鬱',
				DateTime = DateTime.Parse("2017-08-20 20:34:56"),
				Decimal = 23567890.56m,
				Double = 234.56546342,
				Float = 65.2344f,
				Guid = Guid.NewGuid(),
				Int = 55232,
				Long = 63465463456345634,
				SByte = -120,
				Short = 4231,
				String = "lorem ipsum",
				TimeSpan = TimeSpan.FromMinutes(43),
				UInt = 2323423423,
				ULong = 2345234232345423452,
				UShort = 15340
			};
			JsonConvert.Serialize(data).ShouldBeEquivalentTo(Newton.SerializeObject(data));
		}

		[Fact]
		public void Tuple() => JsonConvert.Serialize((a: 1, b: 2, c: 3)).ShouldBeEquivalentTo("{\"Item1\":1,\"Item2\":2,\"Item3\":3}");

		[Fact]
		public void CamelCase() => JsonConvert
			.Serialize((1,2,3), new SerializationSettings { CamelCaseNames = true })
			.ShouldBeEquivalentTo("{\"item1\":1,\"item2\":2,\"item3\":3}");

		[Fact]
		public void StructType()
		{
			var data = new StructType
			{
				Value =  100,
				Value2 = 200
			};
			JsonConvert.Serialize(data).ShouldBeEquivalentTo(@"{""Value"":100,""Value2"":200}");
		}

		private class PrivateDataClass
		{
			public int Value { get; set; }
			private int _int = 10;
			private string Foo { get; set; } = "foo";
			private static bool _static = true;
			private static string BarStatic { get; set; } = "bar";
		}

		[Fact]
		public void PrivatePartsNotTouched() => JsonConvert.Serialize(new PrivateDataClass {Value = 5}).ShouldBeEquivalentTo("{\"Value\":5}");

		[Fact]
		public void PublicFields() => JsonConvert
			.Serialize(new PublicFields { A = true, B = 6, C = 'g' })
			.ShouldBeEquivalentTo("{\"A\":true,\"B\":6,\"C\":\"g\"}");

		[Fact]
		public void NullProperties()
		{
			var input = new NullableProperties
			{
				Text = "test"
			};
			JsonConvert.Serialize(input).ShouldBeEquivalentTo(Newton.SerializeObject(input));
		}

		[Fact]
		public void NullStruct() => JsonConvert.Serialize(new NullablePropertyStruct {Value = 7}).ShouldBeEquivalentTo("{\"Value\":7,\"Property\":null}");

		[Fact]
		public void NullSubStruct() => JsonConvert.Serialize(new StructWithNullableSubStruct { Value = 7 }).ShouldBeEquivalentTo("{\"Value\":7,\"Field\":null}");

		[Fact]
		public void NullFilledSubStruct() => JsonConvert
			.Serialize(new StructWithNullableSubStruct { Value = 7, Field = new StructType() })
			.ShouldBeEquivalentTo("{\"Value\":7,\"Field\":{\"Value\":0,\"Value2\":0}}");

		[Fact]
		public void ArrayOfNulls() => JsonConvert.Serialize(new List<StructType?> {null, null}).ShouldBeEquivalentTo("[null,null]");

		private class GameData
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public IReadOnlyCollection<ReleaseInfo> Releases { get; set; }
			public int ReleaseCount => Releases.Count;
			public string Notes { get; set; }
		}

		private class ReleaseInfo
		{
			public DateTime Date { get; set; }
			public string Region { get; set; }
			public IEnumerable<string> Platforms { get; set; }
		}
	}
}