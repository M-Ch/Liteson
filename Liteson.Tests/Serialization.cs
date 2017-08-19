using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Newton = Newtonsoft.Json.JsonConvert;

namespace JsonSad.Tests
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
							Date = DateTime.Now,
							Platforms = new[] {"psx", "pc", "xbox one"},
							Region = "eu"
						},
						new ReleaseInfo
						{
							Date = DateTime.Now.AddDays(10),
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
							Date = DateTime.Now.AddYears(2),
							Region = "eu"
						}
					}
				}
			};

			JsonConvert.Serialize(data);
			Newton.SerializeObject(data);
		}

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