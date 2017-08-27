using System.Runtime.CompilerServices;

namespace Liteson
{
	internal struct StringBuffer
	{
		public string Text { get; }
		public int Position { get; private set; }
		private readonly int _length;

		public int Line { get; private set; }
		public int Column { get; private set; }

		public StringBuffer(string text)
		{
			Text = text;
			Position = 0;
			Line = 1;
			Column = 1;
			_length = text.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char Peek() => Position < _length ? Text[Position] : '\uffff';

		public char Read()
		{
			if (Position >= _length)
				return '\uffff';
			var value = Peek();
			Position++;
			if(value == '\n')
			{
				Line++;
				Column = 1;
			}
			else
				Column++;
			return value;
		}

		public BufferSnapshot TakeSnapshot() => new BufferSnapshot
		{
			Column = Column,
			Line = Line,
			Position = Position
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance() => Position++;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAtEnd() => Position >= _length;

		public void LoadSnapshot(BufferSnapshot snapshot)
		{
			Line = snapshot.Line;
			Column = snapshot.Column;
			Position = snapshot.Position;
		}
	}

	internal class BufferSnapshot
	{
		public int Line { get; set; }
		public int Column { get; set; }
		public int Position { get; set; }
	}

}