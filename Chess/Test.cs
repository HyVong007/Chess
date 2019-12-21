using System;
using Chess;
using Chess.KingChess;
using System.Threading.Tasks;


public class Test
{
	static void Main()
	{
		var b = new Board(Hehe);
		b.Print();


		//var FILES = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
		//for (int file = 0; file < 8; ++file)
		//	for (int rank = 0; rank < 8; ++rank)
		//	{
		//		var mailBox = new (Color, PieceName)?[8][];
		//		for (int x = 0; x < 8; ++x) mailBox[x] = new (Color, PieceName)?[8];
		//		mailBox[file][rank] = (Color.White, PieceName.King);
		//		var b = new Board(mailBox);
		//		Console.WriteLine("================================================");
		//		Console.WriteLine($"{FILES[file]}{rank + 1}");
		//		b.FindPseudoLegalMoves(Color.White, PieceName.King).PrintColorBinary8x8();
		//		Console.WriteLine("\n");
		//	}

	}


	static async void Ham(Board b)
	{
		var d = await Board.MoveData.New(b, (0, 1), (0, 2));
		Console.WriteLine(d);
	}


	static async Task<PieceName?> Hehe(Color color)
	{
		return null;
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


		for (int x = 6; x < 8; ++x)
			for (int y = 0; y < 8; ++y) a[x][y] = 1;





		#endregion

		char[] INT_CHAR = new char[] { '0', '1' };
		char[] result = new char[64];
		for (int y = a[0].Length - 1, index = 0; y >= 0; --y)
			for (int x = a.Length - 1; x >= 0; --x, ++index)
				result[index] = INT_CHAR[a[x][y]];

		return Convert.ToUInt64(new string(result), fromBase: 2);
	}
}