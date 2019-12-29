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



	public readonly struct Rect
	{
		public readonly int xMin, yMin, xMax, yMax;


		public Rect(int xMin, int yMin, int xMax, int yMax)
		{
			this.xMin = xMin; this.yMin = yMin; this.xMax = xMax; this.yMax = yMax;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(int x, int y) => xMin <= x && x <= xMax && yMin <= y && y <= yMax;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains((int x, int y) index) => Contains(index.x, index.y);
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

		/// <summary>
		/// Chơi theo luật Cờ Úp ?
		/// </summary>
		private readonly bool hiddenChessRule;


		private Board(History<MoveData> history, Piece?[][] mailBox, bool? hidden)
		{
			history.execute += Move;
			for (int x = 0; x < 9; ++x)
			{
				this.mailBox[x] = new Piece?[10];
				for (int y = 0; y < 10; ++y)
				{
					if (mailBox[x][y] == null) continue;
					var square = mailBox[x][y].Value;
					this.mailBox[x][y] = new Piece() { color = square.color, name = square.name, hidden = hidden != null ? hidden.Value : square.hidden };
					if (square.hidden) hiddenChessRule = true;
					if (square.name == PieceName.General) generalIndexes[(int)square.color] = (x, y);
				}
			}
			if (hidden == true) hiddenChessRule = true;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Board NewStandard(History<MoveData> history) => new Board(history, DEFAULT_MAILBOX, hidden: false);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Board NewHidden(History<MoveData> history) => new Board(history, DEFAULT_MAILBOX, hidden: true);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Board NewCustom(History<MoveData> history, Piece?[][] mailBox) => new Board(history, mailBox, hidden: null);


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


		#region FindPseudoLegalMoves
		private static readonly Rect BOARD = new Rect(0, 0, 8, 9);
		private static readonly Rect[] SIDES = new Rect[]
		{
			new Rect(0, 0, 8, 4),	// Color.Red
			new Rect(0, 5, 8, 9)	// Color.Blue
		}, PALACES = new Rect[]
		{
			new Rect(3, 0, 5, 2),	// Color.Red
			new Rect(3, 7, 5, 9)	// Color.Blue
		};
		private static readonly (int x, int y)[] LRUD_VECTORS = new (int x, int y)[]
		{
			(-1, 0), (1, 0), (0, 1), (0, -1)
		},
			CROSS_VECTORS = new (int x, int y)[]
		{
			(-1, -1), (1, 1), (-1, 1), (1, -1)
		};
		private static readonly ((int x, int y) line, (int x, int y)[] crosses)[] HORSE_VECTORS = new ((int x, int y) line, (int x, int y)[] crosses)[]
		{
			(line: (-1, 0), crosses: new (int x, int y)[]{(-1, 1), (-1, -1)}),	// L
			(line: (1, 0), crosses: new (int x, int y)[]{(1, 1), (1, -1)}),		// R
			(line: (0, 1), crosses: new (int x, int y)[]{(-1, 1), (1, 1)}),		// U
			(line: (0, -1), crosses: new (int x, int y)[]{(-1, -1), (1, -1)})	// D
		};
		private static readonly (int x, int y)[] COLOR_FORWARD_VECTORS = new (int x, int y)[]
		{
			(0, 1),	// Color.Red
			(0, -1)	// Color.Blue
		};


		private (int x, int y)[] FindPseudoLegalMoves(Color color, PieceName name, (int x, int y) index)
		{
			list.Clear();
			int x, y;
			switch (name)
			{
				case PieceName.General:
					#region General
					for (int d = 0; d < 4; ++d)
					{
						x = index.x + LRUD_VECTORS[d].x; y = index.y + LRUD_VECTORS[d].y;
						if (PALACES[(int)color].Contains(x, y) && mailBox[x][y]?.color != color) list.Add((x, y));
					}
					break;
				#endregion

				case PieceName.Advisor:
					#region Advisor
					for (int d = 0; d < 4; ++d)
					{
						x = index.x + CROSS_VECTORS[d].x; y = index.y + CROSS_VECTORS[d].y;
						if (((!hiddenChessRule && PALACES[(int)color].Contains(x, y))
							|| (hiddenChessRule && BOARD.Contains(x, y)))
							&& mailBox[x][y]?.color != color) list.Add((x, y));
					}
					break;
				#endregion

				case PieceName.Elephant:
					#region Elephant
					for (int d = 0; d < 4; ++d)
					{
						var dir = CROSS_VECTORS[d];
						x = index.x + dir.x; y = index.y + dir.y;
						if ((!hiddenChessRule && !SIDES[(int)color].Contains(x, y))
							|| (hiddenChessRule && !BOARD.Contains(x, y))
							|| mailBox[x][y] != null) continue;

						x += dir.x; y += dir.y;
						if (((!hiddenChessRule && SIDES[(int)color].Contains(x, y))
							|| (hiddenChessRule && BOARD.Contains(x, y)))
							&& mailBox[x][y]?.color != color) list.Add((x, y));
					}
					break;
				#endregion

				case PieceName.Horse:
					#region Horse
					for (int h = 0; h < 4; ++h)
					{
						var (line, crosses) = HORSE_VECTORS[h];
						x = index.x + line.x; y = index.y + line.y;
						if (!BOARD.Contains(x, y) || mailBox[x][y] != null) continue;

						for (int c = 0; c < 2; ++c)
						{
							x += crosses[c].x; y += crosses[c].y;
							if (BOARD.Contains(x, y) && mailBox[x][y]?.color != color) list.Add((x, y));
						}
					}
					break;
				#endregion

				case PieceName.Rook:
					#region Rook
					for (int d = 0; d < 4; ++d)
					{
						var dir = LRUD_VECTORS[d];
						(x, y) = index;
						while (BOARD.Contains(x += dir.x, y += dir.y))
						{
							var c = mailBox[x][y]?.color;
							if (c != color) list.Add((x, y));
							if (c != null) break;
						}
					}
					break;
				#endregion

				case PieceName.Cannon:
					#region Cannon
					for (int d = 0; d < 4; ++d)
					{
						var dir = LRUD_VECTORS[d];
						(x, y) = index;
						while (BOARD.Contains(x += dir.x, y += dir.y))
						{
							var c = mailBox[x][y]?.color;
							if (c == null)
							{
								list.Add((x, y)); continue;
							}

							while (BOARD.Contains(x += dir.x, y += dir.y))
							{
								var c2 = mailBox[x][y]?.color;
								if (c2 == null) continue;

								if (c2 != color) list.Add((x, y));
								break;
							}
							break;
						}
					}
					break;
				#endregion

				case PieceName.Pawn:
					#region Pawn
					var forward = COLOR_FORWARD_VECTORS[(int)color];
					x = index.x + forward.x; y = index.y + forward.y;
					if (BOARD.Contains(x, y) && mailBox[x][y]?.color != color) list.Add((x, y));
					if (SIDES[(int)color].Contains(x, y)) break;
					for (int d = 0; d < 2; ++d)
					{
						x = index.x + LRUD_VECTORS[d].x; y = index.y + LRUD_VECTORS[d].y;
						if (BOARD.Contains(x, y) && mailBox[x][y]?.color != color) list.Add((x, y));
					}
					break;
					#endregion
			}
			return list.ToArray();
		}
		#endregion


		#region FindLegalMoves
		private (int x, int y)[] FindLegalMoves(Color color, PieceName name, (int x, int y) index)
		{
			var moves = FindPseudoLegalMoves(color, name, index);
			if (moves.Length == 0) return Array.Empty<(int x, int y)>();

			list.Clear();
			for (int m = 0; m < moves.Length; ++m)
			{
				var to = moves[m];
				var data = GenerateMoveData(index, to);
				PseudoMove(data, isUndo: false);
				if (!GeneralIsChecked(color)) list.Add(to);
				PseudoMove(data, isUndo: true);
			}
			return list.ToArray();
		}


		public (int x, int y)[] FindLegalMoves((int x, int y) index)
		{
			var piece = mailBox[index.x][index.y].Value;
			return FindLegalMoves(piece.color, !piece.hidden ? piece.name : DEFAULT_MAILBOX[index.x][index.y].Value.name, index);
		}
		#endregion


		#region Move
		public readonly struct MoveData
		{
			public readonly Piece piece;
			public readonly (int x, int y) from, to;
			public readonly Piece? capturedPiece;


			internal MoveData(Piece piece, (int x, int y) from, (int x, int y) to, Piece? capturedPiece)
			{
				this.piece = piece;
				this.from = from;
				this.to = to;
				this.capturedPiece = capturedPiece;
			}
		}


		public MoveData GenerateMoveData((int x, int y) from, (int x, int y) to) =>
			 new MoveData(mailBox[from.x][from.y].Value, from, to, mailBox[to.x][to.y]);


		private void Move(MoveData data, bool isUndo)
		{
			PseudoMove(data, isUndo);

			#region Cập nhật State
			var oldState = states[(int)data.piece.color];
			states[(int)data.piece.color] = State.Normal;
			if (oldState == State.Check) onStateChanged?.Invoke(data.piece.color, State.Normal);

			var opponentColor = data.piece.color == Color.Red ? Color.Blue : Color.Red;
			if (GeneralIsChecked(opponentColor))
			{
				for (int x = 0; x < 9; ++x)
					for (int y = 0; y < 10; ++y)
						if (mailBox[x][y]?.color == opponentColor)
						{
							var piece = mailBox[x][y].Value;
							if (FindLegalMoves(opponentColor, !piece.hidden ? piece.name : DEFAULT_MAILBOX[x][y].Value.name, (x, y)).Length != 0)
							{
								states[(int)opponentColor] = State.Check;
								onStateChanged?.Invoke(opponentColor, State.Check);
								goto END;
							}
						}

				states[(int)opponentColor] = State.CheckMate;
				onStateChanged?.Invoke(opponentColor, State.CheckMate);
			END:;
			}
			#endregion
		}


		private void PseudoMove(MoveData data, bool isUndo)
		{
			if (!isUndo)
			{
				#region DO
				mailBox[data.from.x][data.from.y] = null;
				var p = data.piece;
				p.hidden = false;
				mailBox[data.to.x][data.to.y] = p;
				if (data.piece.name == PieceName.General)
					generalIndexes[(int)data.piece.color] = data.to;
				#endregion
			}
			else
			{
				#region UNDO
				mailBox[data.from.x][data.from.y] = data.piece;
				mailBox[data.to.x][data.to.y] = data.capturedPiece;
				if (data.piece.name == PieceName.General)
					generalIndexes[(int)data.piece.color] = data.from;
				#endregion
			}
		}
		#endregion


		private static readonly List<(int x, int y)> list = new List<(int x, int y)>(90);

		private readonly (int x, int y)[] generalIndexes = new (int x, int y)[2];


		#region State
		public enum State
		{
			Normal, Check, CheckMate
		}
		private State[] states = new State[2];
		public event Action<Color, State> onStateChanged;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public State GetState(Color color) => states[(int)color];


		private bool GeneralIsChecked(Color color)
		{
			var G = generalIndexes[(int)color];
			var opponentColor = color == Color.Red ? Color.Blue : Color.Red;

			#region Kiểm tra lộ mặt Tướng
			var OpponentG = generalIndexes[(int)opponentColor];
			if (G.x == OpponentG.x)
			{
				var m = mailBox[G.x];
				int DIR_Y = COLOR_FORWARD_VECTORS[(int)color].y;
				while (true)
				{
					if ((G.y += DIR_Y) == OpponentG.y) return true;
					if (m[G.y] != null) break;
				}
			}
			#endregion

			for (int x = 0; x < 9; ++x)
				for (int y = 0; y < 10; ++y)
					if (mailBox[x][y]?.color == opponentColor)
					{
						var piece = mailBox[x][y].Value;
						if (piece.name == PieceName.General) continue;
						var moves = FindPseudoLegalMoves(opponentColor, !piece.hidden ? piece.name : DEFAULT_MAILBOX[x][y].Value.name, (x, y));
						for (int m = 0; m < moves.Length; ++m)
							if (moves[m] == G) return true;
					}
			return false;
		}
		#endregion
	}
}