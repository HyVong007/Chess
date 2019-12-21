using System;
using System.Runtime.CompilerServices;


namespace Chess.KingChess
{
	public static class King_Number_Extension
	{
		private static readonly int[][] MAILBOX_TO_BIT = new int[8][];
		private static readonly (int x, int y)[] BIT_TO_MAILBOX = new (int x, int y)[64];


		static King_Number_Extension()
		{
			for (int x = 0; x < 8; ++x) MAILBOX_TO_BIT[x] = new int[8];
			for (int bit = 0, y = 0; y < 8; ++y)
				for (int x = 0; x < 8; ++x, ++bit)
				{
					MAILBOX_TO_BIT[x][y] = bit;
					BIT_TO_MAILBOX[bit] = (x, y);
				}
		}


		/// <summary>
		/// Return: <see cref="string"/> dạng binary trong bàn cờ Vua 8x8.
		/// </summary>
		public static string ToBinary8x8(this ulong u)
		{
			string bin = u.ToBinary();
			string result = "";
			for (int y = 7, start = 7; y >= 0; --y, start += 8)
			{
				result += $"{y + 1}  ";
				for (int x = 0; x < 8; ++x) result += $"  {bin[start - x]}  ";
				result += "\n\n";
			}
			return result + "\n     A    B    C    D    E    F    G    H    \n";
		}


		/// <summary>
		/// In ra nhị phân dạng bàn cờ Vua 8x8 có màu sắc. bit 1 = màu đỏ
		/// </summary>
		public static void PrintColorBinary8x8(this ulong u)
		{
			string bin = u.ToBinary();
			for (int y = 7, start = 7; y >= 0; --y, start += 8)
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write($"{y + 1}  ");
				for (int x = 0; x < 8; ++x)
				{
					char c = bin[start - x];
					Console.ForegroundColor = c == '1' ? ConsoleColor.Red : ConsoleColor.White;
					Console.Write($"  {c}  ");
				}
				Console.Write("\n\n");
			}
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("\n     A    B    C    D    E    F    G    H    ");
			Console.ForegroundColor = ConsoleColor.White;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToBitIndex(this (int x, int y) mailBoxIndex) => MAILBOX_TO_BIT[mailBoxIndex.x][mailBoxIndex.y];


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (int x, int y) ToMailBoxIndex(this int bitIndex) => BIT_TO_MAILBOX[bitIndex];
	}
}