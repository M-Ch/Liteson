using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Liteson
{
	internal static class Formatting
	{
		public static void WriteIsoFormatFast(DateTime dateTime, TextWriter writer, byte[] buffer)
		{
			GetDatePart(dateTime.Ticks, out var year, out var month, out var day);
			WriteFast(year, writer, buffer);
			writer.Write('-');
			WriteDatePartFast(month, writer);
			writer.Write('-');
			WriteDatePartFast(day, writer);
			writer.Write('T');
			WriteDatePartFast(dateTime.Hour, writer);
			writer.Write(':');
			WriteDatePartFast(dateTime.Minute, writer);
			writer.Write(':');
			WriteDatePartFast(dateTime.Second, writer);
			var ms = dateTime.Ticks % TimeSpan.TicksPerSecond;
			if (ms > 0)
			{
				while (ms % 10 == 0)
					ms /= 10;
				writer.Write('.');
				writer.Write(ms);
			}
			if (dateTime.Kind == DateTimeKind.Utc)
				writer.Write('Z');
			else if (dateTime.Kind == DateTimeKind.Local)
			{
				var offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
				writer.Write(offset.Ticks > 0 ? '+' : '-');
				WriteDatePartFast(offset.Hours, writer);
				writer.Write(':');
				WriteDatePartFast(offset.Minutes, writer);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteFast(int value, TextWriter target, byte[] buffer)
		{
			if (value == 0)
			{
				target.Write('0');
				return;
			}
			if (value < 0)
				target.Write('-');

			var length = 0;
			while (value != 0)
			{
				buffer[length++] = (byte)(value > 0 ? value % 10 : -(value % 10));
				value /= 10;
			}

			for (var a = length - 1; a >= 0; a--)
				target.Write((char)('0' + buffer[a]));
		}

		//works good up to 99
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteDatePartFast(int value, TextWriter target)
		{
			var second = value % 10;
			var first = value / 10;
			target.Write((char)('0' + first));
			target.Write((char)('0' + second));
		}

		/* GetDatePart method:
		 * The MIT License (MIT)
			Copyright (c) .NET Foundation and Contributors

			All rights reserved.

			Permission is hereby granted, free of charge, to any person obtaining a copy
			of this software and associated documentation files (the "Software"), to deal
			in the Software without restriction, including without limitation the rights
			to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
			copies of the Software, and to permit persons to whom the Software is
			furnished to do so, subject to the following conditions:

			The above copyright notice and this permission notice shall be included in all
			copies or substantial portions of the Software.

			THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
			IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
			FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
			AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
			LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
			OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
			SOFTWARE.*/
		//https://github.com/dotnet/coreclr/blob/master/src/mscorlib/shared/System/DateTime.cs

		private static readonly int[] DaysToMonth365 = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};
		private static readonly int[] DaysToMonth366 = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366};

		// Number of 100ns ticks per time unit
		private const long TicksPerMillisecond = 10000;
		private const long TicksPerSecond = TicksPerMillisecond * 1000;
		private const long TicksPerMinute = TicksPerSecond * 60;
		private const long TicksPerHour = TicksPerMinute * 60;
		private const long TicksPerDay = TicksPerHour * 24;

		// Number of days in a non-leap year
		private const int DaysPerYear = 365;
		// Number of days in 4 years
		private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
		// Number of days in 100 years
		private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
		// Number of days in 400 years
		private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

		private static void GetDatePart(long ticks, out int year, out int month, out int day)
		{
			// n = number of days since 1/1/0001
			var n = (int)(ticks / TicksPerDay);
			// y400 = number of whole 400-year periods since 1/1/0001
			var y400 = n / DaysPer400Years;
			// n = day number within 400-year period
			n -= y400 * DaysPer400Years;
			// y100 = number of whole 100-year periods within 400-year period
			var y100 = n / DaysPer100Years;
			// Last 100-year period has an extra day, so decrement result if 4
			if(y100 == 4) y100 = 3;
			// n = day number within 100-year period
			n -= y100 * DaysPer100Years;
			// y4 = number of whole 4-year periods within 100-year period
			var y4 = n / DaysPer4Years;
			// n = day number within 4-year period
			n -= y4 * DaysPer4Years;
			// y1 = number of whole years within 4-year period
			var y1 = n / DaysPerYear;
			// Last year has an extra day, so decrement result if 4
			if(y1 == 4) y1 = 3;
			// compute year
			year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;
			// n = day number within year
			n -= y1 * DaysPerYear;
			// dayOfYear = n + 1;
			// Leap year calculation looks different from IsLeapYear since y1, y4,
			// and y100 are relative to year 1, not year 0
			var leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
			var days = leapYear ? DaysToMonth366 : DaysToMonth365;
			// All months have less than 32 days, so n >> 5 is a good conservative
			// estimate for the month
			var m = (n >> 5) + 1;
			// m = 1-based month number
			while(n >= days[m]) m++;
			// compute month and day
			month = m;
			day = n - days[m - 1] + 1;
		}
	}
}