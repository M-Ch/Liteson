using System.IO;
using System.Runtime.CompilerServices;

namespace Liteson
{
	internal class PositionTrackingReader
	{
		private readonly TextReader _reader;
		public int Column { get; private set; }
		public int Line { get; private set; } 

		public PositionTrackingReader(TextReader reader)
		{
			_reader = reader;
			Column = 1;
			Line = 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char Peek() => (char) _reader.Peek();

		public char Read()
		{
			var value = (char) _reader.Read();
			if (value == '\n')
			{
				Line++;
				Column = 1;
			}
			else
				Column++;
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAtEnd() => _reader.Peek() < 0;
	}
}