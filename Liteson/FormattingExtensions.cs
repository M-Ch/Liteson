using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Liteson
{
	internal static class FormattingExtensions
	{
		public static void WriteIsoFormatFast(this DateTime dateTime, StringWriter writer)
		{
			GetDatePart(dateTime.Ticks, out var year, out var month, out var day);
			WriteFast(year, writer);
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
			if (dateTime.Millisecond > 0)
			{
				writer.Write('.');
				WriteFast(dateTime.Millisecond, writer);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteFast(this int value, TextWriter target)
		{
			int div;
			for (div = 1; div <= value; div *= 10)
			{
			}

			do
			{
				div /= 10;
				target.Write((char)('0' + (value / div)));
				value %= div;
			} while(value > 0);
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

		//The MIT License (MIT)
		//https://github.com/dotnet/coreclr/blob/master/src/mscorlib/shared/System/DateTime.cs

		private static readonly int[] DaysToMonth365 = {
			0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};
		private static readonly int[] DaysToMonth366 = {
			0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366};

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
			int n = (int)(ticks / TicksPerDay);
			// y400 = number of whole 400-year periods since 1/1/0001
			int y400 = n / DaysPer400Years;
			// n = day number within 400-year period
			n -= y400 * DaysPer400Years;
			// y100 = number of whole 100-year periods within 400-year period
			int y100 = n / DaysPer100Years;
			// Last 100-year period has an extra day, so decrement result if 4
			if(y100 == 4) y100 = 3;
			// n = day number within 100-year period
			n -= y100 * DaysPer100Years;
			// y4 = number of whole 4-year periods within 100-year period
			int y4 = n / DaysPer4Years;
			// n = day number within 4-year period
			n -= y4 * DaysPer4Years;
			// y1 = number of whole years within 4-year period
			int y1 = n / DaysPerYear;
			// Last year has an extra day, so decrement result if 4
			if(y1 == 4) y1 = 3;
			// compute year
			year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;
			// n = day number within year
			n -= y1 * DaysPerYear;
			// dayOfYear = n + 1;
			// Leap year calculation looks different from IsLeapYear since y1, y4,
			// and y100 are relative to year 1, not year 0
			bool leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
			int[] days = leapYear ? DaysToMonth366 : DaysToMonth365;
			// All months have less than 32 days, so n >> 5 is a good conservative
			// estimate for the month
			int m = (n >> 5) + 1;
			// m = 1-based month number
			while(n >= days[m]) m++;
			// compute month and day
			month = m;
			day = n - days[m - 1] + 1;
		}
	}
}