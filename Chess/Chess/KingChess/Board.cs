/*
 * Project này sử dụng System.Threading.Tasks.Task
 * Tùy theo platform cụ thể, có thể thay bằng "Task like type" khác
 */


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace Chess.KingChess
{
	public enum PieceName
	{
		Pawn, Rook, Bishop, Knight, Queen, King
	}



	public enum Color
	{
		White, Black
	}



	public sealed class Board
	{
		private const ulong
			CENTER = 0x1818000000UL,
			EXTENDED_CENTER = 0x3C3C3C3C0000UL,
			FILE_A = 0x101010101010101UL,
			FILE_H = 0x8080808080808080UL,
			FILE_AB = 0x303030303030303UL,
			FILE_GH = 0xC0C0C0C0C0C0C0C0UL,
			RANK_1 = 0xFFUL,
			RANK_4 = 0xFF000000UL,
			RANK_5 = 0xFF00000000UL,
			RANK_8 = 0xFF00000000000000UL;


		#region Khai báo dữ liệu và khởi tạo
		/// <summary>
		/// <c>[(<see cref="int"/>)<see cref="Color"/>][(<see cref="int"/>)<see cref="PieceName"/>]= bitboard</c><br/>
		/// Nhớ update <see cref="mailBox"/> sau khi kết thúc modify
		/// </summary>
		private readonly ulong[][] color_piece_bitboards;

		private readonly (Color color, PieceName piece)?[][] mailBox;

		private static readonly (Color color, PieceName piece)?[][] DEFAULT_MAILBOX = new (Color color, PieceName piece)?[][]
		{
			// A
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.Rook), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.Rook)},

			// B
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.Knight), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.Knight)},

			// C
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.Bishop), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.Bishop)},
			
			// D
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.Queen), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.Queen)},

			// E
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.King), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.King)},

			// F
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.Bishop), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.Bishop)},

			// G
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.Knight), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.Knight)},

			// H
			new (Color color, PieceName piece)?[]{(Color.White, PieceName.Rook), (Color.White, PieceName.Pawn), null, null, null, null, (Color.Black, PieceName.Pawn), (Color.Black, PieceName.Rook)},
		};

		private readonly History<MoveData> history;


		///<summary>
		/// Cần kiểm tra mailbox !
		///</summary>
		/// <param name="getPromotedPiece">Pawn Promotion GUI: hiện UI cho người chơi chọn 1 quân để phong cấp (ngoại trừ Vua và Tốt).</param>
		public Board(History<MoveData> history, Func<Color, Task<PieceName?>> getPromotedPiece, (Color color, PieceName piece)?[][] mailBox)
		{
			(this.history = history).execute += Move;
			this.getPromotedPiece = getPromotedPiece;
			for (int x = 0; x < 8; ++x) this.mailBox[x] = new (Color color, PieceName piece)?[8];

			#region Khởi tạo {color_piece_bitboards} và {mailBox}
			color_piece_bitboards = new ulong[2][];
			int piece_length = Enum.GetValues(typeof(PieceName)).Length;
			foreach (Color color in Enum.GetValues(typeof(Color))) color_piece_bitboards[(int)color] = new ulong[piece_length];
			for (int ROW = mailBox.Length, COL = mailBox[0].Length, y = 0, index = 0; y < COL; ++y)
				for (int x = 0; x < ROW; ++x, ++index)
				{
					if ((this.mailBox[x][y] = mailBox[x][y]) == null) continue;
					var (color, piece) = mailBox[x][y].Value;
					color_piece_bitboards[(int)color][(int)piece].SetBit(index);
				}
			#endregion
		}


		///<summary>
		/// Cần kiểm tra mailbox !
		///</summary>
		/// <param name="getPromotedPiece">Pawn Promotion GUI: hiện UI cho người chơi chọn 1 quân để phong cấp (ngoại trừ Vua và Tốt).</param>
		public Board(History<MoveData> history, Func<Color, Task<PieceName?>> getPromotedPiece) : this(history, getPromotedPiece, DEFAULT_MAILBOX) { }
		#endregion


		#region Debug: GUI Display
		public override string ToString()
		{
			string s = "";
			for (int y = mailBox[0].Length - 1; y >= 0; --y)
			{
				s += $" {y + 1}  ";
				for (int x = 0; x < mailBox.Length; ++x)
				{
					var d = mailBox[x][y];
					s += d != null ? $"  {(d.Value.color == Color.White ? PIECENAME_STRING_DICT[d.Value.piece] : PIECENAME_STRING_DICT[d.Value.piece].ToLower())}  " : "  *  ";
				}
				s += "\n";
			}
			s += "\n      A    B    C    D    E    F    G    H ";
			return s;
		}


		public void Print()
		{
			Console.WriteLine($"turn= {history.turn}");
			for (int y = mailBox[0].Length - 1; y >= 0; --y)
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write($"{y + 1}  ");
				for (int x = 0; x < mailBox.Length; ++x)
				{
					var d = mailBox[x][y];
					if (d != null)
					{
						Console.ForegroundColor = d.Value.color == Color.White ? ConsoleColor.White : ConsoleColor.DarkCyan;
						Console.Write($"  {PIECENAME_STRING_DICT[d.Value.piece]}  ");
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						Console.Write("  *  ");
					}
				}
				Console.WriteLine("\n");
			}

			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("\n     A    B    C    D    E    F    G    H ");
			Console.ForegroundColor = ConsoleColor.White;
		}


		private static readonly IReadOnlyDictionary<PieceName, string> PIECENAME_STRING_DICT = new Dictionary<PieceName, string>
		{
			[PieceName.Pawn] = "P",
			[PieceName.Rook] = "R",
			[PieceName.Knight] = "N",
			[PieceName.Bishop] = "B",
			[PieceName.Queen] = "Q",
			[PieceName.King] = "K"
		};
		#endregion


		#region Properties và Indexers: truy vấn (query) các Bitboards
		/// <summary>
		/// Bitboard tập hợp những quân cờ cùng màu.<br/>
		/// Bit 1 = có quân cờ
		/// </summary>
		public ulong this[Color color]
		{
			get
			{
				ulong result = 0UL;
				for (int y = 0; y < 6; ++y) result |= color_piece_bitboards[(int)color][y];
				return result;
			}
		}


		/// <summary>
		/// Bitboard tập hợp các quân cờ cùng tên và màu.<br/>
		/// Bit 1 = có quân cờ
		/// </summary>
		public ulong this[Color color, PieceName piece] => color_piece_bitboards[(int)color][(int)piece];


		/// <summary>
		/// Ô tại vị trí (x, y) trên bàn cờ.
		/// </summary>
		public (Color color, PieceName piece)? this[int x, int y] => mailBox[x][y];


		/// <summary>
		/// Bitboard tập hợp các quân cờ (bất kỳ).<br/>
		/// Bit 1 = có quân cờ
		/// </summary>
		public ulong occupied => this[Color.White] | this[Color.Black];


		/// <summary>
		/// Bitboard tập hợp các ô trống.<br/>
		/// Bit 1 = ô trống
		/// </summary>
		public ulong empty => ~this[Color.White] & ~this[Color.Black];
		#endregion


		#region FindSlicingMoves
		private enum Direction
		{
			Left = 0, Right = 1, Up = 2, Down = 3,
			LeftDown = 4, RightUp = 5, LeftUp = 6, RightDown = 7
		}
		private const bool LEFT_SHIFT = true, RIGHT_SHIFT = false;

		/// <summary>
		/// Constant Data cho di chuyển quân cờ theo <see cref="Direction"/>
		/// <para><c>[(<see cref="int"/>)<see cref="Direction"/>]=data</c>:</para>
		/// <para>NOT_BORDER: Tập hợp các ô không phải biên của <see cref="Direction"/> trên bàn cờ. Nếu quân cờ nằm trên biên của <see cref="Direction"/> thì không thể di chuyển.</para>
		/// <para>SHIFT: Nên dùng toán tử &lt;&lt; hay &gt;&gt; ? </para>
		/// <para>STEP: Số bước shift <c>(&lt;&lt;STEP hoặc &gt;&gt;STEP)</c></para>
		/// </summary>
		private static readonly (ulong NOT_BORDER, bool SHIFT, int STEP)[] EIGHT_DIRECTIONS_DATA = new (ulong NOT_BORDER, bool SHIFT, int STEP)[]
		{
			/*
			 * HORIZONTAL AND VERTICAL
			 * */
			/*Left*/ (~FILE_A, RIGHT_SHIFT, 1),
			/*Right*/ (~FILE_H, LEFT_SHIFT, 1),
			/*Up*/ (~RANK_8, LEFT_SHIFT, 8),
			/*Down*/ (~RANK_1, RIGHT_SHIFT, 8),

			/*
			 * DIAGONAL AND ANTI-DIAGONAL
			 */
			/*LeftDown*/ (~FILE_A&~RANK_1, RIGHT_SHIFT, 9),
			/*RightUp*/ (~FILE_H&~RANK_8, LEFT_SHIFT, 9),
			/*LeftUp*/ (~FILE_A&~RANK_8, LEFT_SHIFT, 7),
			/*RightDown*/ (~FILE_H&~RANK_1, RIGHT_SHIFT, 7),
		};


		/// <summary>
		/// Tìm các ô có thể di chuyển của quân cờ theo quy tắc:<br/>
		///- Có thể đi qua ô trống<br/>
		///- Bị chặn bởi ô chứa quân bất kỳ<br/>
		///- Nếu ô chứa quân khác màu thì ô là 1 move (ăn quân)
		///<para> (bit 1 = move-square)</para>
		/// </summary>
		/// <param name="source">Tập hợp quân cờ cần tìm moves</param>
		/// <param name="EMPTY">Tập hợp ô trống</param>
		/// <param name="EMPTY_OR_OPPONENT">Tập hợp ô trống hoặc chứa quân cờ đối phương (khác màu với quân cờ trong source)</param>
		///<param name="MAX_STEP">Số nước đi liên tiếp tối đa (MAX_STEP &gt; 0)</param>
		private static ulong FindSlicingMoves(ulong source, Direction direction, ulong EMPTY, ulong EMPTY_OR_OPPONENT, byte MAX_STEP = byte.MaxValue)
		{
			ulong result = 0UL;
			var (NOT_BORDER, SHIFT, STEP) = EIGHT_DIRECTIONS_DATA[(int)direction];
			byte step = 0;
			while (true)
			{
				source &= NOT_BORDER;
				if (source == 0) break;
				source = SHIFT == LEFT_SHIFT ? source << STEP : source >> STEP;
				result |= source & EMPTY_OR_OPPONENT;
				if (++step == MAX_STEP) break;
				source &= EMPTY;
			}
			return result;
		}
		#endregion


		/// <summary>
		/// Tìm tập hợp các ô có thể di chuyển của quân cờ theo luật của từng loại quân cờ. Chưa kiểm tra xem King color có bị chiếu.
		/// <para>(bit 1 = move-square)</para>
		/// </summary>
		/// <param name="index"><c>if index !=null:</c> Chỉ trả về tập hợp moves của quân cờ tại index.</param>
		private ulong FindPseudoLegalMoves(Color color, PieceName piece, int? index = null)
		{
			ulong SOURCE = index != null ? color_piece_bitboards[(int)color][(int)piece].GetBit(index.Value) : color_piece_bitboards[(int)color][(int)piece];
			if (SOURCE == 0) return 0UL;
			ulong result = 0UL;
			ulong EMPTY = empty;
			ulong OPPONENT = color == Color.White ? this[Color.Black] : this[Color.White];
			ulong EMPTY_OR_OPPONENT = EMPTY | OPPONENT;

			switch (piece)
			{
				case PieceName.Pawn:
					#region Pawn moves
					if (color == Color.White)
					{
						#region White Pawn Moves: dịch chuyển bằng Left Shift <<
						ulong P_ls = SOURCE << 7;
						result |= P_ls & OPPONENT & ~FILE_H;     // ăn chéo trái
						P_ls <<= 1;
						ulong forward1 = P_ls & EMPTY;              // đi về trước 1 bước
						result |= forward1;
						result |= forward1 << 8 & EMPTY & RANK_4;   // đi về trước 2 bước
						P_ls <<= 1;
						result |= P_ls & OPPONENT & ~FILE_A;     // ăn chéo phải		
						#endregion
					}
					else
					{
						#region Black Pawn Moves: dịch chuyển bằng Right Shift >>
						ulong P_rs = SOURCE >> 7;
						result |= P_rs & OPPONENT & ~FILE_A;     // ăn chéo trái
						P_rs >>= 1;
						ulong forward1 = P_rs & EMPTY;              // đi về trước 1 bước
						result |= forward1;
						result |= forward1 >> 8 & EMPTY & RANK_5;   // đi về trước 2 bước
						P_rs >>= 1;
						result |= P_rs & OPPONENT & ~FILE_H;     // ăn chéo phải
						#endregion
					}

					// Enpassant
					var d = history.lastActionData;
					if (d == null) return result;
					var data = d.Value;
					if (data.color != color && data.piece == PieceName.Pawn
						&& Math.Abs(data.from - data.to) == 16)
					{
						ulong EP = color_piece_bitboards[(int)(color == Color.White ? Color.Black : Color.White)][(int)PieceName.Pawn].GetBit(data.to);
						if (((SOURCE << 1) & EP) != 0 || ((SOURCE >> 1) & EP) != 0)
							result.SetBit(data.color == Color.White ? data.from + 8 : data.to + 8);
					}
					return result;
				#endregion

				case PieceName.Rook:
					#region Rook moves
					for (int i = 0; i < 4; ++i) result |= FindSlicingMoves(SOURCE, (Direction)i, EMPTY, EMPTY_OR_OPPONENT);
					return result;
				#endregion

				case PieceName.Bishop:
					#region Bishop moves
					for (int i = 4; i < 8; ++i) result |= FindSlicingMoves(SOURCE, (Direction)i, EMPTY, EMPTY_OR_OPPONENT);
					return result;
				#endregion

				case PieceName.Queen:
					#region Queen moves
					for (int i = 0; i < 8; ++i) result |= FindSlicingMoves(SOURCE, (Direction)i, EMPTY, EMPTY_OR_OPPONENT);
					return result;
				#endregion

				case PieceName.Knight:
					#region Knight moves
					// U targets ( Left shift << )
					ulong K = SOURCE;
					result |= (K <<= 6) & ~FILE_GH; // L-L-U
					result |= (K <<= 4) & ~FILE_AB; // R-R-U
					result |= (K <<= 5) & ~FILE_H;  // U-U-L
					result |= (K <<= 2) & ~FILE_A;  // U-U-R

					// D targets ( Right shift >> ) 
					K = SOURCE;
					result |= (K >>= 6) & ~FILE_AB; // R-R-D
					result |= (K >>= 4) & ~FILE_GH; // L-L-D
					result |= (K >>= 5) & ~FILE_A;  // D-D-R
					result |= (K >>= 2) & ~FILE_H;  // D-D-L

					return result & EMPTY_OR_OPPONENT;
				#endregion

				case PieceName.King:
					#region King moves
					for (int i = 0; i < 8; ++i) result |= FindSlicingMoves(SOURCE, (Direction)i, EMPTY, EMPTY_OR_OPPONENT, 1);

					// Castling
					if (color_kingMoveCount[(int)color] != 0) return result;
					ulong R = color_piece_bitboards[(int)color][(int)PieceName.Rook];
					var RCount = color_index_rookMoveCount[(int)color];
					if (color == Color.White)
					{
						#region White King
						// Near Castling
						if (EMPTY.IsBit1(5) && EMPTY.IsBit1(6) && R.IsBit1(7) && RCount[7] == 0) result.SetBit(6);
						// Far Castling
						if (EMPTY.IsBit1(3) && EMPTY.IsBit1(2) && EMPTY.IsBit1(1) && R.IsBit1(0) && RCount[0] == 0) result.SetBit(2);
						#endregion
					}
					else
					{
						#region Black King
						// Near Castling
						if (EMPTY.IsBit1(61) && EMPTY.IsBit1(62) && R.IsBit1(63) && RCount[63] == 0) result.SetBit(62);
						// Far Castling
						if (EMPTY.IsBit1(59) && EMPTY.IsBit1(58) && EMPTY.IsBit1(57) && R.IsBit1(56) && RCount[56] == 0) result.SetBit(58);
						#endregion
					}
					return result;
				#endregion

				default: throw new Exception();
			}
		}


		#region FindLegalMoves: Tìm các nước đi hợp lệ sau khi đã kiểm tra King xem có bị chiếu.
		private static readonly List<int> list = new List<int>(64);

		/// <summary>
		/// Tìm các ô có thể move tới của các quân cờ (color, piece).
		/// <para>Đã kiểm tra an toàn: Vua vẫn an toàn ngay sau khi move.</para>
		/// <para>Return: tọa độ Bit Index các ô đi được và an toàn</para>
		/// </summary>
		/// <param name="index"><c>if index != null:</c> Chỉ tìm các moves của quân cờ tại index.</param>
		/// <returns></returns>
		private int[] FindLegalMoves(Color color, PieceName piece, int? index)
		{
			list.Clear();
			if (index != null)
			{
				ulong moves = FindPseudoLegalMoves(color, piece, index);
				if (moves == 0) return Array.Empty<int>();

				int[] to = moves.Bit1_To_Index();
				for (int i = 0, from = index.Value; i < to.Length; ++i) if (LegalMove(from, to[i])) list.Add(to[i]);
				return list.ToArray();
			}

			ulong SOURCE = color_piece_bitboards[(int)color][(int)piece];
			if (SOURCE == 0) return Array.Empty<int>();

			int[] froms = SOURCE.Bit1_To_Index();
			for (int f = 0; f < froms.Length; ++f)
			{
				ulong moves = FindPseudoLegalMoves(color, piece, froms[f]);
				if (moves == 0) continue;

				int[] to = moves.Bit1_To_Index();
				for (int t = 0, from = froms[f]; t < to.Length; ++t) if (LegalMove(from, to[t])) list.Add(to[t]);
			}
			return list.ToArray();


			bool LegalMove(int from, int to)
			{
				var data = NewMoveData(color, piece, from, to, out bool pawnPromotion);
				data.promotedPiece = pawnPromotion ? PieceName.Queen : (PieceName?)null;
				PseudoMove(data, isUndo: false);
				bool isChecked = KingIsChecked(color);
				PseudoMove(data, isUndo: true);
				return !isChecked;
			}
		}


		/// <summary>
		/// Tìm các ô có thể move tới của quân cờ tại index.
		/// <para>Đã kiểm tra an toàn: Vua vẫn an toàn ngay sau khi move.</para>
		/// </summary>
		public int[] FindLegalMoves(int index)
		{
			var (x, y) = index.ToMailBoxIndex();
			var (color, piece) = mailBox[x][y].Value;
			return FindLegalMoves(color, piece, index);
		}


		/// <summary>
		/// Tìm các ô có thể move tới của quân cờ tại (x, y).
		/// <para>Đã kiểm tra an toàn: Vua vẫn an toàn ngay sau khi move.</para>
		/// </summary>
		public (int x, int y)[] FindLegalMoves(int x, int y)
		{
			var (color, piece) = mailBox[x][y].Value;
			int[] moves = FindLegalMoves(color, piece, (x, y).ToBitIndex());
			if (moves.Length == 0) return Array.Empty<(int x, int y)>();

			var result = new (int x, int y)[moves.Length];
			for (int i = 0; i < moves.Length; ++i) result[i] = moves[i].ToMailBoxIndex();
			return result;
		}
		#endregion


		#region Move
		public struct MoveData
		{
			public int from, to;
			public Color color;
			public PieceName piece;
			public PieceName? capturedPiece;

			/// <summary>
			/// Số nước đi của quân Xe bị bắt.
			/// </summary>
			public int? capturedRook_moveCount;

			/// <summary>
			/// Bắt Tốt qua đường (Enpassant): index của quân đối phương bị bắt thông qua enpassant.
			/// </summary>
			public int? enpassantCapturedIndex;

			/// <summary>
			/// Quân sẽ được phong cấp (promotion).<br/>
			/// Không thể phong cấp thành <see cref="PieceName.Pawn"/> hoặc <see cref="PieceName.King"/>
			/// <para><c><see cref="piece"/> == <see cref="PieceName.Pawn"/></c> và tọa độ <see cref="to"/> nằm ở rank cuối cùng (<see cref="RANK_1"/> hoặc <see cref="RANK_8"/>)</para>
			/// </summary>
			public PieceName? promotedPiece;

			public enum Castling
			{
				None, Near, Far
			}
			public Castling castling;


			public override string ToString() => $"color= {color}, piece= {piece}, from= {from.ToMailBoxIndex()}, to= {to.ToMailBoxIndex()}, capturedPiece= {capturedPiece}, " +
					$"enpassantCapturedIndex= {enpassantCapturedIndex?.ToMailBoxIndex()}, promotedPiece= {promotedPiece}, " +
					$"capturedRook_moveCount= {capturedRook_moveCount}, castling= {castling}";
		}

		/// <summary>
		/// Khi nhập thành: Color Rook di chuyển từ "from" tới "to"
		/// <para><c>COLOR_CASTLING_ROOK_MOVEMENTS[(<see cref="int"/>)<see cref="Color"/>][(<see cref="int"/>)<see cref="MoveData.Castling"/>] == (from, to)</c></para>
		/// </summary>
		private static readonly (int from, int to, (int x, int y) m_from, (int x, int y) m_to)[][] COLOR_CASTLING_ROOK_MOVEMENTS = new (int from, int to, (int x, int y) m_from, (int x, int y) m_to)[][]
		{
			// Color.White
			new (int from, int to, (int x, int y) m_from, (int x, int y) m_to)[]
			{
				(from:7, to:5, m_from:7.ToMailBoxIndex(), m_to:5.ToMailBoxIndex()),	// Castling.Near
				(from:0, to:3, m_from:0.ToMailBoxIndex(), m_to:3.ToMailBoxIndex())	// Castling.Far
			},

			// Color.Black
			new (int from, int to, (int x, int y) m_from, (int x, int y) m_to)[]
			{
				(from:63, to:61, m_from:63.ToMailBoxIndex(), m_to:61.ToMailBoxIndex()),	// Castling.Near
				(from:56, to:59, m_from:56.ToMailBoxIndex(), m_to:59.ToMailBoxIndex())	// Castling.Far
			}
		};

		private readonly Func<Color, Task<PieceName?>> getPromotedPiece;


		/// <summary>
		/// Nhớ cập nhật <see cref="MoveData.promotedPiece"/>
		/// </summary>
		private MoveData NewMoveData(Color color, PieceName piece, int from, int to, out bool pawnPromotion)
		{
			var m_to = to.ToMailBoxIndex();
			var data = new MoveData()
			{
				color = color,
				piece = piece,
				from = from,
				to = to,
				capturedPiece = mailBox[m_to.x][m_to.y]?.piece
			};

			if (piece == PieceName.Pawn)
			{
				// Kiểm tra Pawn Promotion
				if ((color == Color.White && to >= 56)
					|| (color == Color.Black && to <= 7))
				{
					pawnPromotion = true;
					return data;
				}

				// Kiểm tra Enpassant
				if (data.capturedPiece == null)
					data.enpassantCapturedIndex = color == Color.White ?
					(from + 7 == to ? from - 1 : from + 9 == to ? from + 1 : (int?)null)
					: (from - 7 == to ? from + 1 : from - 9 == to ? from - 1 : (int?)null);

				if (data.enpassantCapturedIndex != null)
				{
					var (x, y) = data.enpassantCapturedIndex.Value.ToMailBoxIndex();
					data.capturedPiece = mailBox[x][y].Value.piece;
				}
			}
			else if (piece == PieceName.King)
				data.castling = color == Color.White ?
					(to == 6 ? MoveData.Castling.Near : to == 2 ? MoveData.Castling.Far : MoveData.Castling.None)
					: (to == 62 ? MoveData.Castling.Near : to == 58 ? MoveData.Castling.Far : MoveData.Castling.None);

			pawnPromotion = false;
			return data;
		}


		public async Task<MoveData?> GenerateMoveData((int x, int y) from, (int x, int y) to)
		{
			var (color, piece) = mailBox[from.x][from.y].Value;
			var data = NewMoveData(color, piece, from.ToBitIndex(), to.ToBitIndex(), out bool pawnPromotion);
			if (data.capturedPiece == PieceName.Rook)
				data.capturedRook_moveCount = color_index_rookMoveCount[(int)(data.color == Color.White ? Color.Black : Color.White)][data.to];

			return !pawnPromotion ? data : (data.promotedPiece = await getPromotedPiece(data.color)) != null ? data : (MoveData?)null;
		}


		private void Move(MoveData data, bool isUndo)
		{
			PseudoMove(data, isUndo);

			#region Cập nhật state
			var oldState = state[(int)data.color];
			state[(int)data.color] = State.Normal;
			if (oldState == State.Check) onStateChanged?.Invoke(data.color, State.Normal);

			var opponentColor = data.color == Color.White ? Color.Black : Color.White;
			if (KingIsChecked(opponentColor))
			{
				for (int p = 0; p < 6; ++p)
					if (FindLegalMoves(opponentColor, (PieceName)p, null).Length != 0)
					{
						state[(int)opponentColor] = State.Check;
						onStateChanged?.Invoke(opponentColor, State.Check);
						goto END;
					}

				state[(int)opponentColor] = State.CheckMate;
				onStateChanged?.Invoke(opponentColor, State.CheckMate);
			END:;
			}
			#endregion

			#region Cập nhật {color_kingMoveCount} và {color_index_rookMoveCount}
			if (!isUndo)
			{
				#region DO
				if (data.piece == PieceName.King)
					color_kingMoveCount[(int)data.color] = color_kingMoveCount[(int)data.color] < int.MaxValue ? color_kingMoveCount[(int)data.color] + 1 : 1;

				var rookCount = color_index_rookMoveCount[(int)data.color];
				if (data.promotedPiece == PieceName.Rook) rookCount[data.to] = 1;
				if (data.castling != MoveData.Castling.None)
					rookCount[COLOR_CASTLING_ROOK_MOVEMENTS[(int)data.color][(int)data.castling].to] = 1;
				else if (data.piece == PieceName.Rook) rookCount[data.to] = rookCount[data.to] < int.MaxValue ? rookCount[data.from] + 1 : 1;
				#endregion
			}
			else
			{
				#region UNDO
				if (data.piece == PieceName.King) --color_kingMoveCount[(int)data.color];

				var rookCount = color_index_rookMoveCount[(int)data.color];
				if (data.castling != MoveData.Castling.None)
					rookCount[COLOR_CASTLING_ROOK_MOVEMENTS[(int)data.color][(int)data.castling].from] = 0;
				else if (data.piece == PieceName.Rook) rookCount[data.from] = rookCount[data.to] - 1;

				if (data.capturedPiece == PieceName.Rook)
					color_index_rookMoveCount[(int)(data.color == Color.White ? Color.Black : Color.White)][data.to] = data.capturedRook_moveCount.Value;
				#endregion
			}
			#endregion
		}


		/// <summary>
		/// Đi 1 nước/ Hủy 1 nước. Chỉ tác động đến <see cref="color_piece_bitboards"/> và <see cref="mailBox"/>
		/// <para>Có thể dùng để test và hủy nước đi sau khi test.</para>
		/// </summary>
		private void PseudoMove(MoveData data, bool isUndo)
		{
			ulong PIECE = color_piece_bitboards[(int)data.color][(int)data.piece];
			ulong OPPONENT_PIECE = data.capturedPiece != null ? color_piece_bitboards[(int)(data.color == Color.White ? Color.Black : Color.White)][(int)data.capturedPiece] : 0UL;
			var from = data.from.ToMailBoxIndex();
			var to = data.to.ToMailBoxIndex();

			if (!isUndo)
			{
				#region DO
				PIECE.ClearBit(data.from);
				mailBox[from.x][from.y] = null;

				#region Đặt quân {data.piece} vào ô vị trí {to}
				mailBox[to.x][to.y] = (data.color, data.promotedPiece != null ? data.promotedPiece.Value : data.piece);
				if (data.promotedPiece != null)
					color_piece_bitboards[(int)data.color][(int)data.promotedPiece.Value].SetBit(data.to);
				else PIECE.SetBit(data.to);
				#endregion

				#region Xử lý nếu có bắt quân đối phương
				if (data.capturedPiece != null)
					if (data.enpassantCapturedIndex != null)
					{
						// Bắt Tốt Qua Đường
						int bitIndex = data.enpassantCapturedIndex.Value;
						OPPONENT_PIECE.ClearBit(bitIndex);
						var (x, y) = bitIndex.ToMailBoxIndex();
						mailBox[x][y] = null;
					}
					else OPPONENT_PIECE.ClearBit(data.to);
				#endregion

				if (data.castling != MoveData.Castling.None)
				{
					var r = COLOR_CASTLING_ROOK_MOVEMENTS[(int)data.color][(int)data.castling];
					color_piece_bitboards[(int)data.color][(int)PieceName.Rook].ClearBit(r.from);
					mailBox[r.m_from.x][r.m_from.y] = null;
					color_piece_bitboards[(int)data.color][(int)PieceName.Rook].SetBit(r.to);
					mailBox[r.m_to.x][r.m_to.y] = (data.color, PieceName.Rook);
				}
				#endregion
			}
			else
			{
				#region UNDO
				PIECE.SetBit(data.from);
				mailBox[from.x][from.y] = (data.color, data.piece);

				#region Lấy quân {data.piece} hoặc {data.promotedPiece} ra khỏi ô vị trí {to}
				if (data.promotedPiece != null)
					color_piece_bitboards[(int)data.color][(int)data.promotedPiece.Value].ClearBit(data.to);
				else PIECE.ClearBit(data.to);
				#endregion

				#region Khôi phục lại quân đối phương bị bắt nếu có
				if (data.capturedPiece != null)
				{
					var opponentColor = data.color == Color.White ? Color.Black : Color.White;
					if (data.enpassantCapturedIndex != null)
					{
						mailBox[to.x][to.y] = null;
						var bitIndex = data.enpassantCapturedIndex.Value;
						OPPONENT_PIECE.SetBit(bitIndex);
						var (x, y) = bitIndex.ToMailBoxIndex();
						mailBox[x][y] = (opponentColor, data.capturedPiece.Value);
					}
					else
					{
						OPPONENT_PIECE.SetBit(data.to);
						mailBox[to.x][to.y] = (opponentColor, data.capturedPiece.Value);
					}
				}
				else mailBox[to.x][to.y] = null;
				#endregion

				if (data.castling != MoveData.Castling.None)
				{
					var r = COLOR_CASTLING_ROOK_MOVEMENTS[(int)data.color][(int)data.castling];
					color_piece_bitboards[(int)data.color][(int)PieceName.Rook].SetBit(r.from);
					mailBox[r.m_from.x][r.m_from.y] = (data.color, PieceName.Rook);
					color_piece_bitboards[(int)data.color][(int)PieceName.Rook].ClearBit(r.to);
					mailBox[r.m_to.x][r.m_to.y] = null;
				}
				#endregion
			}

			color_piece_bitboards[(int)data.color][(int)data.piece] = PIECE;
			if (data.capturedPiece != null) color_piece_bitboards[(int)(data.color == Color.White ? Color.Black : Color.White)][(int)data.capturedPiece] = OPPONENT_PIECE;
		}
		#endregion


		/// <summary>
		/// Dùng để kiểm tra Nhập Thành (Castling).
		/// <para><c>color_kingMoveCount[(<see cref="int"/>)<see cref="Color"/>] == moveCount_of_King</c></para> 
		/// </summary>
		private readonly int[] color_kingMoveCount = new int[2];

		/// <summary>
		/// Dùng để kiểm tra Nhập Thành (Castling).
		/// <para><c>color_index_rookMoveCount[(<see cref="int"/>)<see cref="Color"/>][index] == moveCount_of_Rook</c></para>
		/// </summary>
		private readonly int[][] color_index_rookMoveCount = new int[][]
		{
			new int[64], new int[64]
		};


		#region State: trạng thái bàn cờ: Vua bên Color có bị chiếu hay chiếu bí
		public enum State
		{
			Normal, Check, CheckMate
		}

		/// <summary>
		/// <c>state[(<see cref="int"/>)<see cref="Color"/>] == state</c>
		/// </summary>
		private readonly State[] state = new State[2];
		public event Action<Color, State> onStateChanged;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public State GetState(Color color) => state[(int)color];


		///<summary>
		/// Vua có bị chiếu ?
		///</summary>
		/// <param name="color">Màu của quân Vua đang kiểm tra.</param>
		/// <returns></returns>
		private bool KingIsChecked(Color color)
		{
			ulong king = color_piece_bitboards[(int)color][(int)PieceName.King];
			var opponentColor = color == Color.White ? Color.Black : Color.White;
			for (int i = 0; i < 6; ++i)
				if ((king & FindPseudoLegalMoves(opponentColor, (PieceName)i)) != 0) return true;
			return false;
		}
		#endregion
	}
}