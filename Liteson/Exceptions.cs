using System;

namespace Liteson
{
	internal static class Exceptions
	{
		public static Exception BadToken(JsonReader reader, JsonToken got, JsonToken expected)
			=> new JsonException($"Unexpected token type ({got}) near line {reader.Line}, column {reader.Column}. Expected {expected}.");

		public static Exception BadFormat(JsonReader reader, string expectedType)
			=> new JsonException($"Expected {expectedType} near line {reader.Line}, column {reader.Column}.");
	}
}