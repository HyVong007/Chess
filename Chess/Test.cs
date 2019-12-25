using System;
using Chess;
using Chess.KingChess;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;


public class Test
{
	static Board board;
	static History<Board.MoveData> history;

	static void Main()
	{
		history = new History<Board.MoveData>();
		board = new Board(history);
		board.getPromotedPiece += async (Color color) => PieceName.Queen;
		board.Print();


		Task.WaitAll(Ham());
	}


	static Dictionary<char, int> dict = new Dictionary<char, int>
	{
		['0'] = 0,
		['1'] = 1,
		['2'] = 2,
		['3'] = 3,
		['4'] = 4,
		['5'] = 5,
		['6'] = 6,
		['7'] = 7,
		['8'] = 8,
		['9'] = 9
	};




	static async Task Ham()
	{
		while (true)
		{
			string cmd = Console.ReadLine();
			switch (cmd[0])
			{
				case ' ':
					Console.WriteLine("Print:");
					board.Print();
					break;

				case 'p':
					var color = cmd[1] == 'w' ? Color.White : Color.Black;
					var from = (dict[cmd[2]], dict[cmd[3]]);
					var to = (dict[cmd[4]], dict[cmd[5]]);

					var data = await Board.MoveData.New(board, from, to);
					history.Play((int)color, data.Value);
					Console.WriteLine("\nAfter Playing:");
					board.Print();
					break;

				case 'f':
					var result = board.FindLegalMoves(dict[cmd[1]], dict[cmd[2]]);
					ToNumber(result).PrintColorBinary8x8();
					break;
			}
		}
	}


	static ulong ToNumber((int x, int y)[] array)
	{
		ulong result = 0UL;
		foreach (var i in array) result.SetBit(i.ToBitIndex());
		return result;
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