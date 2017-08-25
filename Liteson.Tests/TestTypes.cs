using System;
using System.Collections.Generic;

namespace Liteson.Tests
{
	public class BasicTypesBag
	{
		public bool Bool { get; set; }
		public char Char { get; set; }
		public byte Byte { get; set; }
		public sbyte SByte { get; set; }
		public short Short { get; set; }
		public ushort UShort { get; set; }
		public int Int { get; set; }
		public uint UInt { get; set; }
		public long Long { get; set; }
		public ulong ULong { get; set; }
		public double Double { get; set; }
		public float Float { get; set; }
		public decimal Decimal { get; set; }
		public Guid Guid { get; set; }
		public TimeSpan TimeSpan { get; set; }
		public DateTime DateTime { get; set; }
		public byte[] Bytes { get; set; }
		public string String { get; set; }
	}

	public struct StructType
	{
		public int Value { get; set; }
		public int Value2 { get; set; }
	}

	public class SimplePoco
	{
		public bool IsImportant { get; set; }
		public string Value { get; set; }
	}

	public class WithList
	{
		public int Value { get; set; }
		public IReadOnlyList<SimplePoco> Items { get; set; }
	}

	public class PublicFields
	{
		public bool A;
		public int B;
		public char C;
	}

	public class NullableProperties
	{
		public int? Int { get; set; }
		public bool? Bool { get; set; }
		public string Text { get; set; }
	}

	public class NullablePropertyStruct
	{
		public int Value { get; set; }
		public StructType? Property { get; set; }
	}

	public struct StructWithNullableSubStruct
	{
		public StructType? Field;
		public int Value { get; set; }
	}

	public class WithCustomName
	{
		[JsonProperty("f")]
		public string Foo { get; set; }

		[JsonProperty("v")] public int Value;
	}

	public enum SimpleEnum
	{
		Foo,
		Bar,
		Baz
	}

	[Flags]
	public enum FlagEnum
	{
		Flag1 = 1,
		Flag2 = 8,
		All = Flag1 | Flag2
	}

	public class WithEnum
	{
		public SimpleEnum Simple { get; set; }
	}

	public class WithNullableField
	{
		public SimpleEnum? Simple;
	}

	public class NullableEnum
	{
		public SimpleEnum? Simple { get; set; }
	}

	public struct StructWithNullableEnumField
	{
		public SimpleEnum? Simple;
	}

	public enum LongEnum : long
	{
		First = 324523452343243,
		Second = 1241234123423
	}

}