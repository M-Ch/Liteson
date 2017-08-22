using System;
using System.Runtime.CompilerServices;

namespace Liteson
{
	internal static class ValueSkipper
	{
		private const JsonToken AtomicTokens =
			JsonToken.True | JsonToken.False |
			JsonToken.Number | JsonToken.NumberExponent | JsonToken.NumberFloat | JsonToken.NumberNegative |
			JsonToken.String | JsonToken.Null;

		public static void SkipNext(JsonReader reader)
		{
			var part = new BufferPart();
			var token = reader.Read(ref part, out var _);
			SkipFromCurrent(reader, token);
		}

		private static void SkipFromCurrent(JsonReader reader, JsonToken current)
		{
			if(AtomicTokens.HasFlag(current))
				return;

			switch (current)
			{
				case JsonToken.ArrayStart: SkipAsArray(reader); return;
				case JsonToken.ObjectStart: SkipAsObject(reader); return;
			}

			throw Exceptions.BadToken(reader, current, JsonToken.ArrayStart | JsonToken.ObjectStart);
		}

		private static void SkipAsObject(JsonReader reader)
		{
			var part = new BufferPart();
			var firstPass = true;
			while (true)
			{
				var token = reader.Read(ref part, out var _);
				if(token == JsonToken.ObjectEnd && firstPass)
					return;
				if (token != JsonToken.String)
					throw Exceptions.BadToken(reader, token, JsonToken.String);

				AssertToken(JsonToken.NameSeparator, reader, ref part);
				SkipNext(reader);
				token = reader.Read(ref part, out var _);
				if (token == JsonToken.ObjectEnd)
					return;
				if(token != JsonToken.ValueSeparator)
					throw Exceptions.BadToken(reader, token, JsonToken.ValueSeparator);
				firstPass = false;
			}
		}

		private static void SkipAsArray(JsonReader reader)
		{
			var part = new BufferPart();
			var firstPass = true;
			while(true)
			{
				var token = reader.Read(ref part, out var _);
				if(token == JsonToken.ArrayEnd && firstPass)
					return;
				SkipFromCurrent(reader, token);
				token = reader.Read(ref part, out var _);
				if(token == JsonToken.ArrayEnd)
					return;
				if(token != JsonToken.ValueSeparator)
					throw Exceptions.BadToken(reader, token, JsonToken.ValueSeparator);
				firstPass = false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void AssertToken(JsonToken expected, JsonReader reader, ref BufferPart buffer)
		{
			var token = reader.Read(ref buffer, out var _);
			if (!token.HasFlag(expected))
				throw Exceptions.BadToken(reader, token, expected);
		}
	}
}