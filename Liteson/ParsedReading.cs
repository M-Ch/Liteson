using System;
using System.Globalization;

namespace Liteson
{
	internal static class ParsedReading
	{
		public static int ReadInt(JsonReader reader)
		{
			var part = new BufferPart();
			var token = reader.Read(ref part, out var buffer);
			int value;
			if (token == JsonToken.String)
				return int.TryParse(buffer, out value) ? value : throw Exceptions.BadFormat(reader, "integer value");

			if (!token.HasFlag(JsonToken.Number))
				throw Exceptions.BadToken(reader, token, JsonToken.Number);
			if (token.HasFlag(JsonToken.NumberFloat) || token.HasFlag(JsonToken.NumberExponent))
				throw Exceptions.BadFormat(reader, "integer value");

			return Parsing.TryParseBase10Int(part.Text, part.Start, part.Length, out value)
				? value
				: throw Exceptions.BadFormat(reader, "integer value");
		}

		public static string ReadString(JsonReader reader)
		{
			var part = new BufferPart();
			var token = reader.Read(ref part, out var buffer);
			if (token == JsonToken.Null)
				return null;
			if (token == JsonToken.String)
				return buffer;
			if (token.HasFlag(JsonToken.Number))
				return part.Text.Substring(part.Start, part.Length);

			throw Exceptions.BadFormat(reader, "string value");
		}

		public static char ReadChar(JsonReader reader)
		{
			var text = ReadString(reader);
			if (text == null || text.Length != 1)
				throw Exceptions.BadFormat(reader, "single character");
			return text[0];
		}

		public static byte ReadByte(JsonReader reader)
		{
			var intValue = ReadInt(reader);
			if(intValue < byte.MinValue || intValue > byte.MaxValue)
				throw Exceptions.BadFormat(reader, "byte value");
			return (byte) intValue;
		}

		public static sbyte ReadSByte(JsonReader reader)
		{
			var intValue = ReadInt(reader);
			if(intValue < sbyte.MinValue || intValue > sbyte.MaxValue)
				throw Exceptions.BadFormat(reader, "signed byte value");
			return (sbyte)intValue;
		}

		public static short ReadShort(JsonReader reader)
		{
			var intValue = ReadInt(reader);
			if(intValue < short.MinValue || intValue > short.MaxValue)
				throw Exceptions.BadFormat(reader, "short value");
			return (short)intValue;
		}

		public static ushort ReadUShort(JsonReader reader)
		{
			var intValue = ReadInt(reader);
			if(intValue < ushort.MinValue || intValue > ushort.MaxValue)
				throw Exceptions.BadFormat(reader, "unsigned short value");
			return (ushort)intValue;
		}

		private const NumberStyles Base10WithExponent = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

		public static float ReadFloat(JsonReader reader) 
			=> ParseNumber(reader, i => float.TryParse(i, Base10WithExponent, CultureInfo.InvariantCulture, out var result) ? result : default(float?));

		public static double ReadDouble(JsonReader reader)
			=> ParseNumber(reader, i => double.TryParse(i, Base10WithExponent, CultureInfo.InvariantCulture, out var result) ? result : default(double?));

		public static decimal ReadDecimal(JsonReader reader)
			=> ParseNumber(reader, i => decimal.TryParse(i, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result) ? result : default(decimal?));

		public static uint ReadUInt(JsonReader reader) => ParseNumber(reader, i => uint.TryParse(i, out var result) ? result : default(uint?));
		public static long ReadLong(JsonReader reader) => ParseNumber(reader, i => long.TryParse(i, out var result) ? result : default(long?));
		public static ulong ReadULong(JsonReader reader) => ParseNumber(reader, i => ulong.TryParse(i, out var result) ? result : default(ulong?));
		public static Guid ReadGuid(JsonReader reader) => ParseString(reader, i => Guid.TryParseExact(i, "D", out var result) ? result : default(Guid?));
		public static DateTime ReadDateTime(JsonReader reader) => ParseString(reader, i => DateTime.TryParse(i, out var result) ? result : default(DateTime?));
		public static TimeSpan ReadTimeSpan(JsonReader reader) => ParseString(reader, i => TimeSpan.TryParse(i, out var result) ? result : default(TimeSpan?));
		public static byte[] ReadByteArray(JsonReader reader) => ParseString(reader, Convert.FromBase64String);

		public static bool ReadBool(JsonReader reader)
		{
			var part = new BufferPart();
			var token = reader.Read(ref part, out var buffer);
			if (token == JsonToken.True)
				return true;
			if (token == JsonToken.False)
				return false;
			if (token.HasFlag(JsonToken.Number))
			{
				if (part.Length == 1)
					return part.Text[part.Start] != '0';
				var text = part.Text.Substring(part.Start, part.Length);
				if(token.HasFlag(JsonToken.NumberFloat))
					return double.TryParse(text, Base10WithExponent, CultureInfo.InvariantCulture, out var value) 
						? value != 0
						: throw Exceptions.BadFormat(reader, "double value");
				return long.TryParse(text, out var result) ? result != 0 : throw Exceptions.BadFormat(reader, "numeric value");
			}
			if (token == JsonToken.String)
				try
				{
					return Convert.ToBoolean(buffer, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					throw Exceptions.BadFormat(reader, "bool value");
				}

			throw Exceptions.BadToken(reader, token, JsonToken.False | JsonToken.True);
		}

		private static T ParseNumber<T>(JsonReader reader, Func<string, T?> parser) where T : struct
		{
			var part = new BufferPart();
			var token = reader.Read(ref part, out var buffer);
			if (token.HasFlag(JsonToken.Number))
				return parser(part.Text.Substring(part.Start, part.Length)) ?? throw Exceptions.BadFormat(reader, "string value");
			if(token != JsonToken.String)
				throw Exceptions.BadFormat(reader, "string value");
			return parser(buffer) ?? throw Exceptions.BadFormat(reader, "string value");
		}

		private static T ParseString<T>(JsonReader reader, Func<string, T?> parser) where T : struct
		{
			var part = new BufferPart();
			var token = reader.Read(ref part, out var buffer);
			if(token != JsonToken.String)
				throw Exceptions.BadFormat(reader, "string value");
			var result = parser(buffer);
			return result ?? throw Exceptions.BadFormat(reader, "string value");
		}

		private static T ParseString<T>(JsonReader reader, Func<string, T> parser) where T : class
		{
			var part = new BufferPart();
			var token = reader.Read(ref part, out var buffer);
			if (token == JsonToken.Null)
				return null;
			if(token != JsonToken.String)
				throw Exceptions.BadFormat(reader, "string value");
			var result = parser(buffer);
			return result ?? throw Exceptions.BadFormat(reader, "string value");
		}
	}
}