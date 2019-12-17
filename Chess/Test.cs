using System;
using Chess;
using Chess.KingChess;


public class Test
{
	static void Main()
	{
		Console.BackgroundColor = ConsoleColor.DarkGreen;
		Console.Clear();

		//var b = new Board(new (Color color, PieceName piece)?[][]
		//{
		//	// A
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},

		//	// B
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},

		//	// C
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},

		//	// D
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},

		//	// E
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},

		//	// F
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},

		//	// G
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},

		//	// H
		//	new (Color color, PieceName piece)?[]{null, null, null, null, null, null, null, null},
		//});
		//b.Print();






		//Console.WriteLine("\nFindPseudoLegalMoves(Color.White, PieceName.Pawn)=\n");
		//b.FindPseudoLegalMoves(Color.White, PieceName.Pawn).PrintColorBinary8x8();


		ulong u = 0b1000_1011_0101_0010UL;
		u.PrintColorBinary();
		u.Reverse(4, 11);
		u.PrintColorBinary();
	}




	static void Ham(ref int a)
	{
		int b = a;
		b = 3;
	}













	/// <summary>
	/// Nhập bằng tay vô {<see cref="int"/>[][] a} theo quy tắc Bàn cờ Vua 8x8 rồi xuất ra số.
	/// </summary>
	static ulong Array8x8ToNumber()
	{
		int[][] a = new int[8][];
		for (int x = 0; x < 8; ++x)
		{
			a[x] = new int[8];
			for (int y = 0; y < 8; ++y) a[x][y] = 0;
		}


		#region Input a[x][y] = ?


		a[0][0] = 1;
		a[1][0] = 1;





		#endregion

		char[] INT_CHAR = new char[] { '0', '1' };
		char[] result = new char[64];
		for (int y = a[0].Length - 1, index = 0; y >= 0; --y)
			for (int x = a.Length - 1; x >= 0; --x, ++index)
				result[index] = INT_CHAR[a[x][y]];

		return Convert.ToUInt64(new string(result), fromBase: 2);
	}
}
