using System;
using System.IO;

namespace JsonSad
{
	internal static class DateTimeExtensions
	{
		public static void WriteIsoFormatFast(this DateTime dateTime, StringWriter writer)
		{
			writer.Write(dateTime.Year);
			writer.Write('-');
			if(dateTime.Month < 10)
				writer.Write('0');
			writer.Write(dateTime.Month);
			writer.Write('-');
			if(dateTime.Day < 10)
				writer.Write('0');
			writer.Write(dateTime.Day);
			writer.Write('T');
			if(dateTime.Hour < 10)
				writer.Write('0');
			writer.Write(':');
			if(dateTime.Minute < 10)
				writer.Write('0');
			writer.Write(dateTime.Minute);
			writer.Write(':');
			if(dateTime.Second < 10)
				writer.Write('0');
			writer.Write(dateTime.Second);
			if (dateTime.Millisecond > 0)
			{
				writer.Write('.');
				writer.Write(dateTime.Millisecond);
			}
		}
	}
}