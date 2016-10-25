using System;
using System.Linq;

namespace MerkleTools
{
	public class HexEncoder
	{
		private static readonly int[] HexValueArray;
		private static readonly string[] HexTbl = Enumerable.Range(0, 256).Select(v => v.ToString("x2")).ToArray();

		public static string Encode(byte[] data)
		{
			return Encode(data, 0, data.Length);
		}

		public static string Encode(byte[] data, int offset, int count)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			var pos = 0;
			var s = new char[2 * count];
			for (var i = offset; i < offset + count; i++)
			{
				var c = HexTbl[data[i]];
				s[pos++] = c[0];
				s[pos++] = c[1];
			}
			return new string(s);
		}

		public static byte[] Decode(string encoded)
		{
			if (encoded == null)
				throw new ArgumentNullException("encoded");
			if (encoded.Length % 2 == 1)
				throw new FormatException("Invalid Hex String");

			var result = new byte[encoded.Length / 2];
			for (int i = 0, j = 0; i < encoded.Length; i += 2, j++)
			{
				var a = IsDigit(encoded[i]);
				var b = IsDigit(encoded[i + 1]);
				if (a == -1 || b == -1)
					throw new FormatException("Invalid Hex String");
				result[j] = (byte)(((uint)a << 4) | (uint)b);
			}
			return result;
		}

		static HexEncoder()
		{
			var hexDigits = new[] { 
				'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a',
				'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F' };
			var hexValues = new byte[] {
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,
				12,13, 14, 15, 10, 11, 12, 13, 14, 15};

			var max = hexDigits.Max();
			HexValueArray = new int[max + 1];
			for (var i = 0; i < HexValueArray.Length; i++)
			{
				var idx = Array.IndexOf(hexDigits, (char)i);
				var value = -1;
				if (idx != -1)
					value = hexValues[idx];
				HexValueArray[i] = value;
			}
		}

		public static int IsDigit(char c)
		{
			return c + 1 <= HexValueArray.Length
				? HexValueArray[c]
				: -1;
		}
	}
}