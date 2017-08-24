using System.IO;
using FluentAssertions;
using Xunit;

namespace Liteson.Tests.JsonWriter
{
	public class Formatting
	{
		private readonly StringWriter _sw;
		private readonly Liteson.JsonWriter _writer;

		public Formatting()
		{
			_sw = new StringWriter();
			_writer = new Liteson.JsonWriter(_sw, new SerializationSettings {Indent = true});
		}

		[Fact]
		public void SimpleObject()
		{
			const string expected = 
@"{
	""foo"":1
}";
			_writer.BeginObject();
			_writer.PropertyName("foo");
			_writer.Write(1);
			_writer.EndObject();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void EmptyObject()
		{
			_writer.BeginObject();
			_writer.EndObject();
			_sw.ToString().ShouldBeEquivalentTo("{\r\n}");
		}

		[Fact]
		public void ObjectWithTwoProperties()
		{
			const string expected =
@"{
	""foo"":1,
	""bar"":""text""
}";
			_writer.BeginObject();
			_writer.PropertyName("foo");
			_writer.Write(1);
			_writer.NextObjectProperty();
			_writer.PropertyName("bar");
			_writer.Write("text");
			_writer.EndObject();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void NestedProperties()
		{
			const string expected =
@"{
	""foo"":{
		""inner"":""value""
	},
	""bar"":""text""
}";
			_writer.BeginObject();
			_writer.PropertyName("foo");
			_writer.BeginObject();
			_writer.PropertyName("inner");
			_writer.Write("value");
			_writer.EndObject();
			_writer.NextObjectProperty();
			_writer.PropertyName("bar");
			_writer.Write("text");
			_writer.EndObject();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void SimpleArray()
		{
			const string expected =
@"[
	1,
	2
]";
			_writer.BeginArray();
			_writer.ArrayItem();
			_writer.Write(1);
			_writer.ArrayItem();
			_writer.Write(2);
			_writer.EndArray();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void ObjectArray()
		{
			const string expected =
@"[
	{
		""foo"":""bar""
	},
	{
		""foo"":""bar""
	}
]";
			_writer.BeginArray();
			_writer.ArrayItem();
			_writer.BeginObject();
			_writer.PropertyName("foo");
			_writer.Write("bar");
			_writer.EndObject();
			_writer.ArrayItem();
			_writer.BeginObject();
			_writer.PropertyName("foo");
			_writer.Write("bar");
			_writer.EndObject();
			_writer.EndArray();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void EmptyArray()
		{
			const string expected =
@"[
]";
			_writer.BeginArray();
			_writer.EndArray();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void NestedArray()
		{
			const string expected =
@"[
	[
		1
	]
]";
			_writer.BeginArray();
			_writer.ArrayItem();
			_writer.BeginArray();
			_writer.ArrayItem();
			_writer.Write(1);
			_writer.EndArray();
			_writer.EndArray();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}

		[Fact]
		public void NestedEmptyArray()
		{
			const string expected =
@"[
	[
	]
]";
			_writer.BeginArray();
			_writer.ArrayItem();
			_writer.BeginArray();
			_writer.EndArray();
			_writer.EndArray();
			_sw.ToString().ShouldBeEquivalentTo(expected);
		}
	}
}