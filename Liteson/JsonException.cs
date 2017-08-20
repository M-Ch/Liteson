using System;

namespace Liteson
{
	public class JsonException : Exception
	{
		public JsonException(string message) : base(message)
		{
		}
	}
}