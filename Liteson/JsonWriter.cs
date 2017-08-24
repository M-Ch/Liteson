using System;
using System.Globalization;
using System.IO;

namespace Liteson
{
	internal class JsonWriter
	{
		private readonly TextWriter _target;
		private int _depth;
		private readonly string _newLine;
		private readonly string _tab;
		private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
		private static readonly string[] Unescapes = new string['\\'+1];
		private int _arrayItemCount;
		private readonly byte[] _buffer = new byte[20];

		static JsonWriter()
		{
			Unescapes['"'] = "\\\"";
			Unescapes['\\'] = @"\\";
			Unescapes['\b'] = @"\b";
			Unescapes['\f'] = @"\f";
			Unescapes['\n'] = @"\n";
			Unescapes['\r'] = @"\r";
			Unescapes['\t'] = @"\t";
		}

		public JsonWriter(TextWriter target, SerializationSettings settings)
		{
			_target = target;
			_tab = settings.Tab;
			_newLine = settings.Indent ? Environment.NewLine : null;
		}

		public void BeginObject() => BeginComplex('{');

		public void PropertyName(string name)
		{
			NewLine();
			Write(name);
			_target.Write(':');
		}

		public void NextObjectProperty() => _target.Write(',');

		public void EndObject() => EndComplex('}');

		public void BeginArray()
		{
			BeginComplex('[');
			_arrayItemCount = 0;
		}

		public void ArrayItem()
		{
			if(_arrayItemCount++ > 0)
				_target.Write(',');
			NewLine();
		}

		public void EndArray() => EndComplex(']');

		public void Write(string text)
		{
			if (text == null)
			{
				_target.Write("null");
				return;
			}
			_target.Write('"');
			foreach (var character in text)
			{
				string unescaped;
				if (character <= '\\' && (unescaped = Unescapes[character]) != null)
					_target.Write(unescaped);
				else
					_target.Write(character);
			}
			_target.Write('"');
		}

		public void Write(char value)
		{
			_target.Write('"');
			_target.Write(value);
			_target.Write('"');
		}

		public void WriteNull() => _target.Write("null");
		public void Write(bool value) => _target.Write(value ? "true" : "false");
		public void Write(byte value) => Formatting.WriteFast(value, _target, _buffer);
		public void Write(sbyte value) => Formatting.WriteFast(value, _target, _buffer);
		public void Write(short value) => Formatting.WriteFast(value, _target, _buffer);
		public void Write(ushort value) => Formatting.WriteFast(value, _target, _buffer);
		public void Write(int value) => Formatting.WriteFast(value, _target, _buffer);
		public void Write(uint value) => _target.Write(value);
		public void Write(long value) => _target.Write(value);
		public void Write(ulong value) => _target.Write(value);
		public void Write(float value) => _target.Write(value.ToString(InvariantCulture));
		public void Write(double value) => _target.Write(value.ToString(InvariantCulture));
		public void Write(decimal value) => _target.Write(value.ToString(InvariantCulture));

		public void Write(DateTime dateTime)
		{
			_target.Write('"');
			Formatting.WriteIsoFormatFast(dateTime, _target, _buffer);
			_target.Write('"');
		}

		public void Write(TimeSpan timeSpan)
		{
			_target.Write('"');
			_target.Write(timeSpan.ToString());
			_target.Write('"');
		}

		public void Write(Guid value)
		{
			_target.Write('"');
			_target.Write(value.ToString());
			_target.Write('"');
		}

		public void Write(byte[] value)
		{
			_target.Write('"');
			Base64.Write(_target, value, _buffer);
			_target.Write('"');
		}

		private void BeginComplex(char symbol)
		{
			_target.Write(symbol);
			_depth++;
		}

		private void EndComplex(char symbol)
		{
			_depth--;
			NewLine();
			_target.Write(symbol);
		}

		private void NewLine()
		{
			if (_newLine == null)
				return;
			_target.Write(_newLine);
			for (var a = 0; a < _depth; a++)
				_target.Write(_tab);
		}
	}
}