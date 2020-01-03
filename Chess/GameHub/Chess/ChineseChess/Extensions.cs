using System;
using System.Runtime.CompilerServices;


namespace GameHub.Chess.ChineseChess
{
	public static class Util
	{
		public static void PrintColorBits(this (int x, int y)[] array)
		{
			for (int y = 9; y >= 0; --y)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write($"{y + 1}	");
				for (int x = 0; x < 9; ++x)
					if (array.Contains((x, y)))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write("   1   ");
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("   0   ");
					}
				Console.WriteLine("\n\n");
			}
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n\n	   A      B      C      D      E      F      G      H      I");
			Console.ForegroundColor = ConsoleColor.White;
		}


		public static bool Contains(this (int x, int y)[] array, (int x, int y) item)
		{
			for (int i = 0; i < array.Length; ++i) if (array[i] == item) return true;
			return false;
		}
	}
}
