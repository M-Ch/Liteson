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
}