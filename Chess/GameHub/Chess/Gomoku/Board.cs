using System;


namespace GameHub.Chess.Gomoku
{
	public enum Symbol
	{
		O = 0, X = 1
	}



	public sealed class Board
	{
		#region Khai báo dữ liệu và khởi tạo
		private readonly Symbol?[][] mailBox;
		private readonly Rect BOARD;

		/// <summary>
		/// Số ô trống trên bàn cờ.
		/// </summary>
		private int emptyCells;


		public Board(byte width, byte height, History<MoveData> history)
		{
			if (width < 5 || height < 5) throw new ArgumentOutOfRangeException("Kích thước (width, heigth) phải >= (5, 5)");
			mailBox = new Symbol?[width][];
			for (int x = 0; x < width; ++x) mailBox[x] = new Symbol?[height];
			BOARD = new Rect(0, 0, width - 1, height - 1);
			emptyCells = width * height;
			history.execute += Move;
		}


		public Board(Symbol?[][] mailBox, History<MoveData> history) : this((byte)mailBox.Length, (byte)mailBox[0].Length, history)
		{
			for (int x = 0; x <= BOARD.xMax; ++x)
				for (int y = 0; y <= BOARD.yMax; ++y)
					if (mailBox[x][y] != null)
					{
						this.mailBox[x][y] = mailBox[x][y];
						--emptyCells;
					}
		}


		public Symbol? this[int x, int y] => mailBox[x][y];
		#endregion


		#region DEBUG: GUI Display
		public override string ToString()
		{
			string s = "";
			for (int y = BOARD.yMax; y >= 0; --y)
			{
				s += $"{y}    ";
				for (int x = 0; x < mailBox.Length; ++x)
				{
					var symbol = mailBox[x][y];
					s += symbol != null ? $"  {(symbol == Symbol.O ? "O" : "X")}  " : "  *  ";
				}
				s += "\n\n";
			}

			s += "\n     ";
			for (int x = 0; x <= BOARD.xMax; ++x) s += $"  {x}  ";
			return s;
		}


		public void Print()
		{
			for (int y = BOARD.yMax; y >= 0; --y)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"{y}    ");
				for (int x = 0; x < mailBox.Length; ++x)
				{
					var symbol = mailBox[x][y];
					Console.ForegroundColor = symbol == Symbol.O ? ConsoleColor.Red : symbol == Symbol.X ? ConsoleColor.Green : ConsoleColor.DarkYellow;
					Console.Write(symbol == Symbol.O ? "  O  " : symbol == Symbol.X ? "  X  " : "  *  ");
				}
				Console.WriteLine("\n");
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("\n     ");
			for (int x = 0; x <= BOARD.xMax; ++x) Console.Write($"  {x}  ");
			Console.WriteLine();
		}
		#endregion


		#region Move
		public readonly struct MoveData
		{
			public readonly Symbol symbol;
			public readonly (int x, int y) index;


			public MoveData(Symbol symbol, (int x, int y) index)
			{
				this.symbol = symbol;
				this.index = index;
			}
		}

		/// <summary>
		/// Vector Phương = { Ngang, Dọc, Chéo Thuận, Chéo Nghịch}
		/// <para><c>AXES[i] == { Direction (Vector Chiều) }</c></para> 
		/// </summary>
		private static readonly (int x, int y)[][] AXES = new (int x, int y)[][]
		{
			new (int x, int y)[]{(-1, 0), (1, 0)},	// Ngang
			new (int x, int y)[]{(0, 1), (0, -1)},	// Dọc
			new (int x, int y)[]{(-1, -1), (1, 1)},	// Chéo Thuận
			new (int x, int y)[]{(-1, 1), (1, -1)},	// Chéo Nghịch
		};


		private void Move(MoveData data, bool isUndo)
		{
			if (!isUndo)
			{
				#region DO
				mailBox[data.index.x][data.index.y] = data.symbol;
				--emptyCells;

				#region Kiểm tra {data.symbol} có chiến thắng hay bàn cờ hòa ?
				var enemySymbol = data.symbol == Symbol.O ? Symbol.X : Symbol.O;
				for (int a = 0; a < 4; ++a)
				{
					var axe = AXES[a];
					winLine[0] = winLine[1] = data.index;
					int count = 1, enemy = 0, lineIndex = 0;
					for (int d = 0; d < 2; ++d)
					{
						var direction = axe[d];
						for (int x = data.index.x + direction.x, y = data.index.y + direction.y; count <= 6 && BOARD.Contains(x, y); x += direction.x, y += direction.y)
						{
							var symbol = mailBox[x][y];
							if (symbol == data.symbol) { winLine[lineIndex] = (x, y); ++count; continue; }
							if (symbol == enemySymbol) ++enemy;
							break;
						}
						if (count > 5) goto CONTINUE_LOOP_AXE;
						++lineIndex;
					}

					if (count == 5 && enemy < 2)
					{
						state = data.symbol == Symbol.O ? State.O_Win : State.X_Win;
						onStateChanged?.Invoke(state);
						break;
					}
				CONTINUE_LOOP_AXE:;
				}

				if (state == State.Normal && emptyCells == 0)
				{
					state = State.Draw;
					onStateChanged?.Invoke(state);
				}
				#endregion
				#endregion
			}
			else
			{
				#region UNDO
				mailBox[data.index.x][data.index.y] = null;
				++emptyCells;

				#region Cập nhật state
				var oldState = state;
				state = State.Normal;
				if (oldState != State.Normal) onStateChanged?.Invoke(state);
				#endregion
				#endregion
			}
		}
		#endregion


		#region State
		public enum State
		{
			Normal, O_Win, X_Win, Draw
		}
		public State state { get; private set; }

		/// <summary>
		/// Đoạn thẳng chiến thắng: nối 5 quân liên tiếp của 1 hàng (ngang, dọc, chéo thuận, chéo nghịch).
		/// <para>2 đầu mút đoạn thẳng: [0] và [1]</para>
		/// </summary>
		private readonly (int x, int y)[] winLine = new (int x, int y)[] { (-1, -1), (-1, -1) };

		public (int x, int y)[] WinLine => new (int x, int y)[] { winLine[0], winLine[1] };

		public event Action<State> onStateChanged;
		#endregion
	}
}