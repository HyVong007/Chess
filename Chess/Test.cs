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
		board = new Board(history, async (Color color) => PieceName.Queen);
		board.Print();


		Task.WaitAll(Ham());
	}


	static Dictionary<char, int> rankToNum = new Dictionary<char, int>
	{
		['1'] = 0,
		['2'] = 1,
		['3'] = 2,
		['4'] = 3,
		['5'] = 4,
		['6'] = 5,
		['7'] = 6,
		['8'] = 7
	};

	static Dictionary<char, int> fileToNum = new Dictionary<char, int>
	{
		['A'] = 0,
		['B'] = 1,
		['C'] = 2,
		['D'] = 3,
		['E'] = 4,
		['F'] = 5,
		['G'] = 6,
		['H'] = 7
	};







	static async Task Ham()
	{
		PrintHelp();
		while (true)
		{
			Console.WriteLine("Enter command:");
			string[] cmd = Console.ReadLine().ToUpper().Split(' ');
			switch (cmd[0])
			{
				case "PLAY":
					(int x, int y) from, to;
					try
					{
						string s = cmd[1];
						from = (fileToNum[s[0]], rankToNum[s[1]]);
						to = (fileToNum[s[2]], rankToNum[s[3]]);
					}
					catch (Exception) { goto default; }
					var data = await Board.MoveData.New(board, from, to);
					history.Play((int)data.Value.color, data.Value);
					Console.WriteLine($"{data.Value.color} play:");
					board.Print();
					break;

				case "FIND":
					int x, y;
					try
					{
						string s = cmd[1];
						x = fileToNum[s[0]]; y = rankToNum[s[1]];
					}
					catch (Exception) { goto default; }
					ToNumber(board.FindLegalMoves(x, y)).PrintColorBinary8x8();
					break;

				case "CANUNDO":
				{
					int id;
					try
					{
						id = cmd[1] == "WHITE" ? 0 : cmd[1] == "BLACK" ? 1 : throw new Exception();
					}
					catch (Exception) { goto default; }
					Console.WriteLine(history.CanUndo(id));
				}
				break;

				case "CANREDO":
				{
					int id;
					try
					{
						id = cmd[1] == "WHITE" ? 0 : cmd[1] == "BLACK" ? 1 : throw new Exception();
					}
					catch (Exception) { goto default; }
					Console.WriteLine(history.CanRedo(id));
				}
				break;

				case "UNDO":
				{
					int id;
					try
					{
						id = cmd[1] == "WHITE" ? 0 : cmd[1] == "BLACK" ? 1 : throw new Exception();
					}
					catch (Exception) { goto default; }
					history.Undo(id);
					board.Print();
				}
				break;

				case "REDO":
				{
					int id;
					try
					{
						id = cmd[1] == "WHITE" ? 0 : cmd[1] == "BLACK" ? 1 : throw new Exception();
					}
					catch (Exception) { goto default; }
					history.Redo(id);
					board.Print();
				}
				break;

				case "STATE":
					Console.WriteLine($"White= {board.GetState(Color.White)}, Black= {board.GetState(Color.Black)}");
					break;

				case "HELP": PrintHelp(); break;
				case "": board.Print(); break;


				default:
					Console.WriteLine("Command not found !");
					break;
			}
		}


		void PrintHelp()
		{
			Console.WriteLine("Help:\n" +
				"play [from_file][from_rank][to_file][to_rank]		Example: play a1h7\n" +
				"find [file][rank]			Example: find a1h7\n" +
				"canundo [color]			Example: canundo white / canundo black\n" +
				"canredo [color]			Example: canredo white / canredo black\n" +
				"undo [color]			Example: undo white / undo black\n" +
				"redo[color]			Example: redo white / redo black\n" +
				"state			The state of White player and Black player\n" +
				"			Empty command (Only Enter): Print the Board !\n" +
				"help			Print this help");
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