using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace GameHub.Chess.GOChess
{
	public enum Color
	{
		White = 0, Black = 1
	}



	internal sealed class Land
	{
		public int airHole;
		public readonly List<(int x, int y)> indexes = new List<(int x, int y)>();
	}



	public sealed class Board
	{
		#region Khai báo dữ liệu và khởi tạo
		private static readonly (int x, int y)[] DIRECTIONS = new (int x, int y)[]
		{
			(-1, 0), (1, 0), (0, 1), (0, -1)
		};
		private readonly List<Land>[] lands = new List<Land>[]
		{
			new List<Land>(), new List<Land>()
		};
		private readonly Piece[][] mailBox;
		private readonly Rect BOARD;


		public Board(byte width, byte height, History<MoveData> history)
		{
			if (width < 2 || height < 2) throw new ArgumentOutOfRangeException("Kích thước (width, heigth) phải >= (2, 2)");
			history.execute += Move;
			mailBox = new Piece[width][];
			for (int x = 0; x < width; ++x) mailBox[x] = new Piece[height];
			BOARD = new Rect(0, 0, width - 1, height - 1);
		}


		public Board(Piece[][] mailBox, History<MoveData> history) : this((byte)mailBox.Length, (byte)mailBox[0].Length, history)
		{
			for (int x = 0; x <= BOARD.xMax; ++x)
				for (int y = 0; y <= BOARD.yMax; ++y)
					this.mailBox[x][y] = mailBox[x][y];
		}
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
					var color = mailBox[x][y]?.color;
					s += color == Color.White ? "  W  " : color == Color.Black ? "  B  " : "  *  ";
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
					var color = mailBox[x][y]?.color;
					Console.ForegroundColor = color == Color.White ? ConsoleColor.Red : color == Color.Black ? ConsoleColor.Green : ConsoleColor.DarkYellow;
					Console.Write(color == Color.White ? "  O  " : color == Color.Black ? "  X  " : "  *  ");
				}
				Console.WriteLine("\n");
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("\n     ");
			for (int x = 0; x <= BOARD.xMax; ++x) Console.Write($"  {x}  ");
			Console.WriteLine();
		}
		#endregion


		#region CanMove
		/// <summary>
		/// <c>[(<see cref="int"/>)<see cref="Color"/>] == { [<see cref="Land"/>] == point}</c>
		/// </summary>
		private static readonly Dictionary<Land, int>[] tmp = new Dictionary<Land, int>[]
		{
				new Dictionary<Land, int>(),
				new Dictionary<Land, int>()
		};

		public bool CanMove(Color color, (int x, int y) index)
		{
			if (mailBox[index.x][index.y] != null) return false;

			tmp[0].Clear();
			tmp[1].Clear();

			for (int d = 0, x, y; d < 4; ++d)
			{
				var direction = DIRECTIONS[d];
				x = index.x + direction.x; y = index.y + direction.y;
				if (!BOARD.Contains(x, y)) goto CONTINUE_LOOP_DIRECTIONS;
				var piece = mailBox[x][y];
				if (piece == null) return true;

				if (!tmp[(int)piece.color].ContainsKey(piece.land)) tmp[(int)piece.color][piece.land] = 1;
				else ++tmp[(int)piece.color][piece.land];
				CONTINUE_LOOP_DIRECTIONS:;
			}

			for (int c = 0; c < 2; ++c)
				foreach (var land_point in tmp[c])
					if ((c == (int)color && land_point.Key.airHole > land_point.Value) || (c != (int)color && land_point.Key.airHole == land_point.Value)) return true;

			return false;
		}
		#endregion


		#region Move
		public readonly struct MoveData
		{
			public readonly (int x, int y) index;
			public readonly Color color;
			internal readonly int emptyHole;
			internal readonly List<Land> dyingEnemies;

			/// <summary>
			/// <c>[<see cref="Land"/>] == airHole của <see cref="Land"/></c>
			/// </summary>
			internal readonly Dictionary<Land, int> enemies, allies;


			/// <summary>
			/// <c>[(<see cref="int"/>)<see cref="Color"/>] == { [<see cref="Land"/>] == point}</c>
			/// </summary>
			private static readonly Dictionary<Land, int>[] lands = new Dictionary<Land, int>[]
			{
				new Dictionary<Land, int>(),
				new Dictionary<Land, int>()
			};

			internal MoveData(Board board, Color color, (int x, int y) index)
			{
				this.index = index;
				this.color = color;
				dyingEnemies = new List<Land>();
				enemies = new Dictionary<Land, int>();
				allies = new Dictionary<Land, int>();
				lands[0].Clear();
				lands[1].Clear();
				emptyHole = 0;

				#region Tìm lổ trống và tất cả land.
				for (int d = 0; d < 4; ++d)
				{
					var direction = DIRECTIONS[d];
					int x = index.x + direction.x, y = index.y + direction.y;
					if (!board.BOARD.Contains(x, y)) continue;
					var piece = board.mailBox[x][y];
					if (piece == null) { ++emptyHole; continue; }

					if (!lands[(int)piece.color].ContainsKey(piece.land)) lands[(int)piece.color][piece.land] = 1;
					else ++lands[(int)piece.color][piece.land];
				}
				#endregion

				#region Xác định land địch sắp chết, land địch còn sống và land mình.
				for (int c = 0; c < 2; ++c)
					foreach (var land_point in lands[c])
						if (c != (int)color && land_point.Key.airHole == land_point.Value) dyingEnemies.Add(land_point.Key);
						else if (c != (int)color) enemies[land_point.Key] = land_point.Value;
						else allies[land_point.Key] = land_point.Value;
				#endregion
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MoveData GenerateMoveData(Color color, (int x, int y) index) => new MoveData(this, color, index);


		private void Move(MoveData data, bool isUndo)
		{
			pieceCounts[0] = pieceCounts[1] = -1;
			var enemyColor = data.color == Color.White ? Color.Black : Color.White;
			if (!isUndo)
			{
				#region DO
				#region Liên kết các land mình hiện tại tạo land mới cho cờ ở {data.index}
				//var piece = Piece.Get(data.color);
				//piece.transform.position = data.index.ArrayToWorld();
				var piece = mailBox[data.index.x][data.index.y] = new Piece(data.color, new Land { airHole = data.emptyHole });
				lands[(int)data.color].Add(piece.land);
				piece.land.indexes.Add(data.index);
				foreach (var land_point in data.allies)
				{
					piece.land.airHole += (land_point.Key.airHole - land_point.Value);
					piece.land.indexes.AddRange(land_point.Key.indexes);
					var indexes = land_point.Key.indexes;
					for (int i = 0, x, y; i < indexes.Count; ++i)
					{
						(x, y) = indexes[i];
						mailBox[x][y].land = piece.land;
					}
					lands[(int)data.color].Remove(land_point.Key);
				}
				#endregion

				#region Trừ lổ thở land địch
				foreach (var enemy_point in data.enemies) enemy_point.Key.airHole -= enemy_point.Value;
				for (int e = data.dyingEnemies.Count - 1; e >= 0; --e)
				{
					var enemy = data.dyingEnemies[e];

					// Giết land địch
					lands[(int)enemyColor].Remove(enemy);

					for (int i = enemy.indexes.Count - 1, x, y; i >= 0; --i)
					{
						(x, y) = enemy.indexes[i];
						//mailBox[x][y].Recycle();
						mailBox[x][y] = null;
						for (int d = 0, __x, __y; d < 4; ++d)
						{
							__x = x + DIRECTIONS[d].x; __y = y + DIRECTIONS[d].y;
							if (!BOARD.Contains(__x, __y)) continue;
							var land = mailBox[__x][__y]?.land;
							if (land != null && land != enemy) ++land.airHole;
						}
					}
				}
				#endregion
				#endregion
			}
			else
			{
				#region UNDO
				//mailBox[data.index.x][data.index.y].Recycle();
				mailBox[data.index.x][data.index.y] = null;

				// Khôi phục con trỏ land mình
				foreach (var land in data.allies.Keys)
					foreach (var (x, y) in land.indexes) mailBox[x][y].land = land;

				// Khôi phục lổ thở land địch còn sống
				foreach (var enemy_point in data.enemies) enemy_point.Key.airHole += enemy_point.Value;

				// Khôi phục land địch bị giết
				for (int e = data.dyingEnemies.Count - 1; e >= 0; --e)
				{
					var enemy = data.dyingEnemies[e];
					lands[(int)enemyColor].Add(enemy);
					for (int i = enemy.indexes.Count - 1, x, y; i >= 0; --i)
					{
						(x, y) = enemy.indexes[i];
						//var piece = piece.Get(enemyColor);
						mailBox[x][y] = new Piece(enemyColor, enemy);
						//piece.transform.position = (x, y).ArrayToWorld();
					}
				}
				#endregion
			}
		}
		#endregion


		#region State
		/// <summary>
		/// <c>[(<see cref="int"/>)<see cref="Color"/>] == count</c>
		/// </summary>
		private readonly int[] pieceCounts = new int[] { -1, -1 };


		public int PieceCount(Color color)
		{
			if (pieceCounts[0] >= 0) return pieceCounts[(int)color];

			int c = 0;
			var list = lands[(int)color];
			for (int i = 0; i < list.Count; ++i) c += list[i].indexes.Count;
			return pieceCounts[(int)color] = c;
		}


		public enum State
		{
			White_Win, Black_Win, Draw
		}
		public State? state { get; private set; }
		public event Action<State> onFinished;


		/// <summary>
		/// Kết thúc ván chơi và quyết định kết quả.
		/// <para>Chú ý: Không thể Undo Finish !</para>
		/// </summary>
		public State Finish()
		{
			int w = PieceCount(Color.White), b = PieceCount(Color.Black);
			state = w > b ? State.White_Win : b > w ? State.Black_Win : State.Draw;
			onFinished?.Invoke(state.Value);
			return state.Value;
		}
		#endregion
	}
}