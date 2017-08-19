using System;
using System.IO;

namespace JsonSad
{
	internal static class Base64
	{
		private static readonly char[] Lookup =
		{
			'A', 'B', 'C', 'D','E','F','G', 'H','I','J','K','L',
			'M','N', 'O','P','Q','R','S','T','U','V','W','X','Y','Z',
			'a','b','c','d','e','f','g','h','i','j','k','l','m','n',
			'o','p','q','r','s','t','u','v','w','x','y','z',
			'0','1','2','3','4','5','6','7','8','9','+','/','='
		};

		public static void Write(StringWriter target, byte[] data)
		{
			var blockCount = data.Length / 3;
			for (var a = 0; a<blockCount;a++)
			{
			}
		}
	}
}