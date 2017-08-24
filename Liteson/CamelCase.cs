using System.Text;

namespace Liteson
{
	internal static class CamelCase
	{
		public static string ToCamelCase(string input)
		{
			//currently only converting FirstUpper to firstLower.
			//todo: other inputs

			if (char.IsLower(input[0]))
				return input;
			var buffer = new StringBuilder();
			buffer.Clear();
			for (var a = 0; a < input.Length; a++)
				buffer.Append(a == 0 ? char.ToLowerInvariant(input[a]) : input[a]);
			return buffer.ToString();
		}
	}
}