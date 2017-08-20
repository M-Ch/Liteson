using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Liteson
{
	internal class JsonReader
	{
		private readonly PositionTrackingReader _reader;
		private static readonly bool[] Whitespace = new bool[' ' + 1];
		private static readonly char[] Escapes = new char['t'+1];

		static JsonReader()
		{
			Whitespace[' '] = true;
			Whitespace['\t'] = true;
			Whitespace['\r'] = true;
			Whitespace['\n'] = true;

			Escapes['"'] = '"';
			Escapes['\\'] = '\\';
			Escapes['b'] = '\b';
			Escapes['f'] = '\f';
			Escapes['n'] = '\n';
			Escapes['r'] = '\r';
			Escapes['t'] = '\t';
		}

		public JsonReader(TextReader reader) => _reader = new PositionTrackingReader(reader);

		private void SkipWhitespace()
		{
			while (true)
			{
				var current = _reader.Peek();
				if (!IsWhitespace(current))
					return;
				_reader.Read();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsWhitespace(char value) => value <= ' ' && Whitespace[value];

		public JsonToken Read(out string buffer)
		{
			buffer = null;
			SkipWhitespace();
			var current = _reader.Peek();
			if (current  == '\uffff')
				return JsonToken.End;

			switch (current)
			{
				case 't':
					ConsumeMatchingContent("true");
					return JsonToken.True;
				case 'f':
					ConsumeMatchingContent("false");
					return JsonToken.False;
				case 'n':
					ConsumeMatchingContent("null");
					return JsonToken.Null;
				case '"':
					_reader.Read();
					buffer = ReadString();
					return JsonToken.String;
				case '[':
					_reader.Read();
					return JsonToken.ArrayStart;
				case ']':
					_reader.Read();
					return JsonToken.ArrayEnd;
				case '{':
					_reader.Read();
					return JsonToken.ObjectStart;
				case '}':
					_reader.Read();
					return JsonToken.ObjectEnd;
				case ':':
					_reader.Read();
					return JsonToken.NameSeparator;
				case ',':
					_reader.Read();
					return JsonToken.ValueSeparator;
				default:
					if (current == '-' || (current >= '0' && current <= '9'))
						return JsonToken.Number | ReadNumber(out buffer);
					throw new JsonException($"Unexpected token '{current}' at line {_reader.Line}, column {_reader.Column}.");
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAtEnd() => _reader.IsAtEnd();

		private JsonToken ReadNumber(out string result)
		{
			//number format as regex: -?(0|[1-9][0-9]*)(.[0-9]+)?([eE][+-]?[0-9]+)?
			//valid numbers: 1, 1.4, 1.45e5, 1.34E-6.4
			//invalid numbers: 01, 1c, 0x1, +5

			var sb = new StringBuilder();
			var modifiers = _reader.Peek() == '-' ? JsonToken.NumberNegative : 0;
			if (modifiers == JsonToken.NumberNegative)
				sb.Append(_reader.Read());
			if (!ReadIntPart(sb, false))
				throw NumericException();

			if (_reader.Peek() == '.')
			{
				sb.Append(_reader.Read());
				modifiers |= JsonToken.NumberFloat;
				if (!ReadIntPart(sb, true))
					throw NumericException();
			}

			var pos = _reader.Peek();
			if (pos == 'e' || pos == 'E')
			{
				sb.Append(_reader.Read());
				modifiers |= JsonToken.NumberExponent;
				var sign = _reader.Peek();
				if (sign == '+' || sign == '-')
					sb.Append(_reader.Read());
				if (!ReadIntPart(sb, true))
					throw NumericException();
			}

			result = sb.ToString();
			return modifiers;
		}

		private Exception NumericException() => new JsonException($"Got numeric value in invalid format near line {_reader.Line}, column {_reader.Column}.");

		private bool ReadIntPart(StringBuilder sb, bool allowZeros)
		{
			//0 or [1-9][0-9]*
			var first = _reader.Read();
			if (first < '0' || first > '9')
				return false;
			sb.Append(first);
			if (!allowZeros && first == '0')
				return true;

			while (true)
			{
				var current = _reader.Peek();
				if (current == '.' || current == 'e' || current == 'E' || IsWhitespace(current))
					return true;
				if (current < '0' || current > '9')
					return _reader.IsAtEnd();
				sb.Append(_reader.Read());
			}
		}

		private string ReadString()
		{
			var sb = new StringBuilder();
			while (true)
			{
				var current = _reader.Read();
				switch (current)
				{
					case '"': return sb.ToString();
					case '\\': sb.Append(ReadEscapedChar()); break;
					default: sb.Append(current); break;
				}
			}
		}

		private char ReadEscapedChar()
		{
			var current = _reader.Read();
			if (current == 'u')
			{
				HexBuffer buffer;
				buffer.A = _reader.Read();
				buffer.B = _reader.Read();
				buffer.C = _reader.Read();
				buffer.D = _reader.Read();
				if (Parsing.TryParseCharHex(ref buffer, out var result))
					return result;
				throw new JsonException($"Invalid unicode escape sequence \\u{buffer} near line {_reader.Line}, column {_reader.Column}.");
			}
			char escaped;
			if (current > 't' || (escaped = Escapes[current]) == 0)
				throw new JsonException($"Invalid escape char '\\{current}' on line {_reader.Line}, column {_reader.Column}.");
			return escaped;
		}

		private void ConsumeMatchingContent(string expected)
		{
			for (var a = 0; a < expected.Length; a++)
			{
				var current = _reader.Read();
				if (expected[a] != current)
					throw new JsonException($"Expected '{expected}' near line {_reader.Line}, column {_reader.Column}, got '{current}' instead.");
			}
		}
	}

	[Flags]
	internal enum JsonToken : short
	{
		False =          0b00000000_00000001,
		Null =           0b00000000_00000010,
		True =           0b00000000_00000100,
		ObjectStart =    0b00000000_00001000,
		ArrayStart =     0b00000000_00010000,
		Number =         0b00000000_00100000,
		NumberFloat =    0b00000000_01000000,
		NumberExponent = 0b00000000_10000000,
		NumberNegative = 0b00000001_00000000,
		ArrayEnd =       0b01000010_00000000,
		String =         0b00000100_00000000,
		NameSeparator =  0b00001000_00000000,
		ValueSeparator = 0b00010000_00000000,
		ObjectEnd =      0b00100000_00000000,
		End =            0b01000000_00000000,
	}
}