using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;


namespace Chess.ChineseChess
{
	public enum Color
	{
		Red = 0, Blue = 1
	}



	public enum PieceName
	{
		General = 0,
		Advisor = 1,
		Elephant = 2,
		Horse = 3,
		Rook = 4,
		Cannon = 5,
		Pawn = 6
	}



	public struct Piece
	{
		public Color color;
		public PieceName name;
		public bool hidden;
	}



	public sealed class Board
	{
		#region Khai báo dữ liệu và khởi tạo
		private readonly Piece?[][] mailBox = new Piece?[9][];
		private static readonly Piece?[][] DEFAULT_MAILBOX = new Piece?[][]
		{
			// FILE A
			new Piece?[]{new Piece(){color=Color.Red, name=PieceName.Rook},null, null, new Piece(){color=Color.Red, name=PieceName.Pawn}, null, null, new Piece(){color=Color.Blue, name=PieceName.Pawn}, null, null, new Piece(){color=Color.Blue, name=PieceName.Rook} },

			// FILE B
			new Piece?[]{ new Piece() { color = Color.Red, name = PieceName.Horse }, null, new Piece() { color = Color.Red, name = PieceName.Cannon }, null, null, null, null, new Piece() { color = Color.Blue, name = PieceName.Cannon }, null, new Piece() { color = Color.Blue, name = PieceName.Horse } },

			// FILE C
			new Piece?[]{ new Piece() { color = Color.Red, name = PieceName.Elephant }, null, null, new Piece() { color = Color.Red, name = PieceName.Pawn }, null, null, new Piece() { color = Color.Blue, name = PieceName.Pawn }, null, null, new Piece() { color = Color.Blue, name = PieceName.Elephant } },

			// FILE D
			new Piece?[]{ new Piece() { color = Color.Red, name = PieceName.Advisor }, null, null, null, null, null, null, null, null, new Piece() { color = Color.Blue, name = PieceName.Advisor } },

			// FILE E
			new Piece?[]{ new Piece() { color = Color.Red, name = PieceName.General }, null, null, new Piece() { color = Color.Red, name = PieceName.Pawn }, null, null, new Piece() { color = Color.Blue, name = PieceName.Pawn }, null, null, new Piece() { color = Color.Blue, name = PieceName.General } },

			// FILE F
			new Piece?[]{ new Piece() { color = Color.Red, name = PieceName.Advisor }, null, null, null, null, null, null, null, null, new Piece() { color = Color.Blue, name = PieceName.Advisor } },

			// FILE G
			new Piece?[]{ new Piece() { color = Color.Red, name = PieceName.Elephant }, null, null, new Piece() { color = Color.Red, name = PieceName.Pawn }, null, null, new Piece() { color = Color.Blue, name = PieceName.Pawn }, null, null, new Piece() { color = Color.Blue, name = PieceName.Elephant } },

			// FILE H
			new Piece?[]{ new Piece() { color = Color.Red, name = PieceName.Horse }, null, new Piece() { color = Color.Red, name = PieceName.Cannon }, null, null, null, null, new Piece() { color = Color.Blue, name = PieceName.Cannon }, null, new Piece() { color = Color.Blue, name = PieceName.Horse } },

			// FILE I
			new Piece?[]{new Piece(){color=Color.Red, name=PieceName.Rook},null, null, new Piece(){color=Color.Red, name=PieceName.Pawn}, null, null, new Piece(){color=Color.Blue, name=PieceName.Pawn}, null, null, new Piece(){color=Color.Blue, name=PieceName.Rook} },
		};


		private Board(Piece?[][] mailBox, bool? hidden)
		{
			for (int x = 0; x < 9; ++x)
			{
				this.mailBox[x] = new Piece?[10];
				for (int y = 0; y < 10; ++y)
				{
					var square = mailBox[x][y];
					if (square == null) continue;
					this.mailBox[x][y] = new Piece() { color = square.Value.color, name = square.Value.name, hidden = hidden != null ? hidden.Value : square.Value.hidden };
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Board NewStandard() => new Board(DEFAULT_MAILBOX, hidden: false);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Board NewHidden() => new Board(DEFAULT_MAILBOX, hidden: true);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Board NewCustom(Piece?[][] mailBox) => new Board(mailBox, hidden: null);


		public static Piece?[][] CloneDefaultMailBox
		{
			get
			{
				var result = new Piece?[9][];
				for (int x = 0; x < 9; ++x)
				{
					result[x] = new Piece?[10];
					for (int y = 0; y < 10; ++y) result[x][y] = DEFAULT_MAILBOX[x][y];
				}
				return result;
			}
		}


		public Piece? this[int x, int y] => mailBox[x][y];
		#endregion


		#region DEBUG: GUI Display
		public override string ToString()
		{
			string s = "";
			for (int y = 9; y >= 0; --y)
			{
				s += $"{y + 1}	";
				for (int x = 0; x < 9; ++x)
				{
					if (mailBox[x][y] == null) { s += "   *   "; continue; }
					var square = mailBox[x][y].Value;
					s += $"   {(square.hidden ? "?" : (square.color == Color.Red ? PIECENAME_STRING_DICT[square.name] : PIECENAME_STRING_DICT[square.name].ToLower()))}   ";
				}
				s += "\n\n\n";
			}
			return s + "\n\n	   A      B      C      D      E      F      G      H      I";
		}


		public void Print()
		{
			for (int y = 9; y >= 0; --y)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write($"{y + 1}	");
				for (int x = 0; x < 9; ++x)
				{
					if (mailBox[x][y] == null)
					{
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						Console.Write("   *   ");
						continue;
					}

					var square = mailBox[x][y].Value;
					if (square.hidden)
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("   ?   ");
					}
					else
					{
						Console.ForegroundColor = square.color == Color.Red ? ConsoleColor.Red : ConsoleColor.Blue;
						Console.Write($"   {PIECENAME_STRING_DICT[square.name]}   ");
					}
				}
				Console.WriteLine("\n\n");
			}
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n\n	   A      B      C      D      E      F      G      H      I");
			Console.ForegroundColor = ConsoleColor.White;
		}


		private static readonly IReadOnlyDictionary<PieceName, string> PIECENAME_STRING_DICT = new Dictionary<PieceName, string>
		{
			[PieceName.General] = "G",
			[PieceName.Advisor] = "A",
			[PieceName.Elephant] = "E",
			[PieceName.Horse] = "H",
			[PieceName.Rook] = "R",
			[PieceName.Cannon] = "C",
			[PieceName.Pawn] = "P"
		};
		#endregion
	}
}