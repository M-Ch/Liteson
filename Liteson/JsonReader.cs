using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Liteson
{
	internal class JsonReader
	{
		private static readonly bool[] Whitespace = new bool[' ' + 1];
		private static readonly char[] Escapes = new char['t'+1];
		private StringBuffer _buffer;
		private readonly StringBuilder _stringBuilder = new StringBuilder();
		public int Line => _buffer.Line;
		public int Column => _buffer.Column;

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

		public JsonReader(string text) => _buffer = new StringBuffer(text);

		private void SkipWhitespace()
		{
			while (true)
			{
				var current = _buffer.Peek();
				if (!IsWhitespace(current))
					return;
				_buffer.Advance();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsWhitespace(char value) => value <= ' ' && Whitespace[value];

		public JsonToken Read(ref BufferPart bufferPart, out string buffer)
		{
			bufferPart.Text = _buffer.Text;
			bufferPart.Start = _buffer.Position;
			bufferPart.Length = -1;
			buffer = null;

			SkipWhitespace();
			var current = _buffer.Peek();
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
					_buffer.Advance();
					buffer = ReadString();
					return JsonToken.String;
				case '[':
					_buffer.Advance();
					return JsonToken.ArrayStart;
				case ']':
					_buffer.Advance();
					return JsonToken.ArrayEnd;
				case '{':
					_buffer.Advance();
					return JsonToken.ObjectStart;
				case '}':
					_buffer.Advance();
					return JsonToken.ObjectEnd;
				case ':':
					_buffer.Advance();
					return JsonToken.NameSeparator;
				case ',':
					_buffer.Advance();
					return JsonToken.ValueSeparator;
				default:
					if (current == '-' || (current >= '0' && current <= '9'))
					{
						var numberType = ReadNumber(out var length);
						bufferPart.Length = length;
						return JsonToken.Number | numberType;
					}
					throw new JsonException($"Unexpected token '{current}' at line {_buffer.Line}, column {_buffer.Column}.");
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAtEnd() => _buffer.IsAtEnd();

		private JsonToken ReadNumber(out int length)
		{
			//number format as regex: -?(0|[1-9][0-9]*)(.[0-9]+)?([eE][+-]?[0-9]+)?
			//valid numbers: 1, 1.4, 1.45e5, 1.34E-6.4
			//invalid numbers: 01, 1c, 0x1, +5

			var start = _buffer.Position;
			var modifiers = _buffer.Peek() == '-' ? JsonToken.NumberNegative : 0;
			if (modifiers == JsonToken.NumberNegative)
				_buffer.Advance();
			if (!EnsureIntPart(false))
				throw NumericException();

			if (_buffer.Peek() == '.')
			{
				_buffer.Advance();
				modifiers |= JsonToken.NumberFloat;
				if (!EnsureIntPart(true))
					throw NumericException();
			}

			var pos = _buffer.Peek();
			if (pos == 'e' || pos == 'E')
			{
				_buffer.Advance();
				modifiers |= JsonToken.NumberExponent;
				var sign = _buffer.Peek();
				if (sign == '+' || sign == '-')
					_buffer.Advance();
				if (!EnsureIntPart(true))
					throw NumericException();
			}

			length = _buffer.Position - start;
			return modifiers;
		}

		private Exception NumericException() => new JsonException($"Got numeric value in invalid format near line {_buffer.Line}, column {_buffer.Column}.");

		private bool EnsureIntPart(bool allowZeros)
		{
			//0 or [1-9][0-9]*
			var first = _buffer.Read();
			if (first < '0' || first > '9')
				return false;
			if (!allowZeros && first == '0')
				return true;

			while (true)
			{
				var current = _buffer.Peek();
				if (current == '.' || current == 'e' || current == 'E' || IsWhitespace(current))
					return true;
				if (current < '0' || current > '9')
					return _buffer.IsAtEnd();
				_buffer.Advance();
			}
		}

		private string ReadString()
		{
			_stringBuilder.Clear();
			while (true)
			{
				var current = _buffer.Read();
				if (current == '\uffff')
					throw new JsonException("Unexpected end of input. String value was not terminated.");
				switch (current)
				{
					case '"': return _stringBuilder.ToString();
					case '\\': _stringBuilder.Append(ReadEscapedChar()); break;
					default: _stringBuilder.Append(current); break;
				}
			}
		}

		private char ReadEscapedChar()
		{
			var current = _buffer.Read();
			if (current == 'u')
			{
				HexBuffer buffer;
				buffer.A = _buffer.Read();
				buffer.B = _buffer.Read();
				buffer.C = _buffer.Read();
				buffer.D = _buffer.Read();
				if (Parsing.TryParseCharHex(ref buffer, out var result))
					return result;
				throw new JsonException($"Invalid unicode escape sequence \\u{buffer} near line {_buffer.Line}, column {_buffer.Column}.");
			}
			char escaped;
			if (current > 't' || (escaped = Escapes[current]) == 0)
				throw new JsonException($"Invalid escape char '\\{current}' on line {_buffer.Line}, column {_buffer.Column}.");
			return escaped;
		}

		private void ConsumeMatchingContent(string expected)
		{
			for (var a = 0; a < expected.Length; a++)
			{
				var current = _buffer.Read();
				if (expected[a] != current)
					throw new JsonException($"Expected '{expected}' near line {_buffer.Line}, column {_buffer.Column}, got '{current}' instead.");
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