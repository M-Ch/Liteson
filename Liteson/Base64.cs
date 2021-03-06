﻿using System;
using System.IO;

namespace Liteson
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

		public static void Write(TextWriter target, byte[] data, byte[] buffer)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			const int blockSize = 3;
			var blockCount = data.Length / blockSize;
			for (var a = 0; a<blockCount;a++)
			{
				var index = a * blockSize;
				target.Write(Lookup[data[index] >> 2]);
				target.Write(Lookup[(data[index] & 0b0000_0011) << 4 | (data[index+1] >> 4)]);
				target.Write(Lookup[(data[index + 1] & 0b0000_1111) << 2 | data[index + 2] >> 6]);
				target.Write(Lookup[data[index + 2] & 0b0011_1111]);
			}

			var remaining = data.Length % blockSize;
			if (remaining == 0)
				return;
			//handcrafted last block:
			Array.Copy(data, blockCount * blockSize, buffer, 0, remaining);
			target.Write(Lookup[buffer[0] >> 2]);
			if (remaining >= 1)
			{
				target.Write(Lookup[(buffer[0] & 0b0000_0011) << 4 | (buffer[1] >> 4)]);
				if(remaining == 2)
					target.Write(Lookup[(buffer[1] & 0b0000_1111) << 2 | buffer[2] >> 6]);
			}

			target.Write(remaining == 1 ? "==" : "=");
		}
	}
}