using System.Runtime.CompilerServices;

namespace Liteson
{
	internal struct HexBuffer
	{
		public char A;
		public char B;
		public char C;
		public char D;
		public override string ToString() => string.Concat(A, B, C, D);
	}

	internal static class Parsing
	{
		public static bool TryParseCharHex(ref HexBuffer buffer, out char result)
		{
			result = '\0';
			if (!HexToInt(buffer.A, out var a)) return false;
			if (!HexToInt(buffer.B, out var b)) return false;
			if (!HexToInt(buffer.C, out var c)) return false;
			if (!HexToInt(buffer.D, out var d)) return false;

			result = (char) (a << 12 | b << 8 | c << 4 | d);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool HexToInt(char hex, out int value)
		{
			if(hex >= '0' && hex <= '9')
			{
				value = hex - '0';
				return true;
			}
			if (hex >= 'A' && hex <= 'F')
			{
				value = hex - 'A';
				return true;
			}
			if(hex >= 'a' && hex <= 'f')
			{
				value = hex - 'a';
				return true;
			}
			value = 0;
			return false;
		}
	}
}