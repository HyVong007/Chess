/*
 * Project này sử dụng System.Threading.Tasks.Task
 * Tùy theo platform cụ thể, có thể thay bằng "Task like type" khác
 */


using System;
using System.Collections.Generic;
using System.Threading.Tasks;


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



	public class Board
	{
		public const ulong
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


		/// <param name="getPromotedPiece">Pawn Promotion GUI: hiện UI cho người chơi chọn 1 quân để phong cấp (ngoại trừ Vua và Tốt).</param>
		public Board((Color color, PieceName piece)?[][] mailBox, Func<Color, Task<PieceName?>> getPromotedPiece)
		{
			this.getPromotedPiece = getPromotedPiece;
			this.mailBox = mailBox;

			#region Khởi tạo {color_piece_bitboards}
			color_piece_bitboards = new ulong[2][];
			int piece_length = Enum.GetValues(typeof(PieceName)).Length;
			foreach (Color color in Enum.GetValues(typeof(Color))) color_piece_bitboards[(int)color] = new ulong[piece_length];
			for (int ROW = mailBox.Length, COL = mailBox[0].Length, y = 0, index = 0; y < COL; ++y)
				for (int x = 0; x < ROW; ++x, ++index)
				{
					if (mailBox[x][y] == null) continue;
					var (color, piece) = mailBox[x][y].Value;
					color_piece_bitboards[(int)color][(int)piece] |= 0UL.SetBit(index);
				}
			#endregion
		}


		/// <param name="getPromotedPiece">Pawn Promotion GUI: hiện UI cho người chơi chọn 1 quân để phong cấp (ngoại trừ Vua và Tốt).</param>
		public Board(Func<Color, Task<PieceName?>> getPromotedPiece) : this(DEFAULT_MAILBOX, getPromotedPiece) { }
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
		/// Tìm tập hợp các ô có thể di chuyển của quân cờ theo luật của từng loại quân cờ. Chưa kiểm tra xem King có bị chiếu.
		/// <para>(bit 1 = move-square)</para>
		/// </summary>
		/// <param name="index"><c>if index !=null:</c> Chỉ trả về tập hợp moves của quân cờ tại index.</param>
		public ulong FindPseudoLegalMoves(Color color, PieceName piece, int? index = null)
		{
			ulong SOURCE = index != null ? color_piece_bitboards[(int)color][(int)piece].GetBit(index.Value) : color_piece_bitboards[(int)color][(int)piece];
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

						// enpassant (Bắt Tốt qua đường) ?
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

						// enpassant (Bắt Tốt qua đường) ?
						#endregion
					}
					return result;
				#endregion

				case PieceName.Rook:
					#region Rook moves
					for (int i = 0; i < 4; ++i) result |= FindSlicingMoves(SOURCE, (Direction)i, EMPTY, EMPTY_OR_OPPONENT);

					// Castling (Nhập thành) ?
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
					return result;
				#endregion

				default: return result;
			}
		}


		#region FindLegalMoves: Tìm các nước đi hợp lệ sau khi đã kiểm tra King xem có bị chiếu.
		/// <summary>
		/// Tìm các ô có thể move tới của các quân cờ (color, piece).
		/// <para>Đã kiểm tra an toàn: Vua vẫn an toàn ngay sau khi move.</para>
		/// </summary>
		/// <param name="index"><c>if index != null:</c> Chỉ tìm các moves của quân cờ tại index.</param>
		/// <returns></returns>
		public ulong FindLegalMoves(Color color, PieceName piece, int? index = null)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Tìm các ô có thể move tới của quân cờ tại (x, y).
		/// <para>Đã kiểm tra an toàn: Vua vẫn an toàn ngay sau khi move.</para>
		/// </summary>
		public (int x, int y)[] FindLegalMoves(int x, int y)
		{
			var (color, piece) = mailBox[x][y].Value;
			ulong moves = FindLegalMoves(color, piece, (x, y).ToBitIndex());
			return Bit1_To_MailBox(moves);
		}


		/// <summary>
		/// Chuyển tọa độ các bit 1 sang tọa độ MailBox.
		/// </summary>
		public static (int x, int y)[] Bit1_To_MailBox(ulong bitboard)
		{
			var result = new List<(int x, int y)>();
			for (int i = 0; i < 64; ++i) if (bitboard.GetBit(i) != 0) result.Add(i.ToMailBoxIndex());
			return result.ToArray();
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
			/// Bắt Tốt qua đường (Enpassant): index của quân đối phương bị bắt thông qua enpassant.
			/// </summary>
			public int? enpassantCapturedIndex;

			/// <summary>
			/// Quân sẽ được phong cấp (promotion).<br/>
			/// Không thể phong cấp thành <see cref="PieceName.Pawn"/> hoặc <see cref="PieceName.King"/>
			/// <para><c><see cref="piece"/> == <see cref="PieceName.Pawn"/></c> và tọa độ <see cref="to"/> nằm ở rank cuối cùng (<see cref="RANK_1"/> hoặc <see cref="RANK_8"/>)</para>
			/// </summary>
			public PieceName? promotedPiece;


			private MoveData(Board board, (int x, int y) from, (int x, int y) to, out bool pawnPromotion)
			{
				(color, piece) = board.mailBox[from.x][from.y].Value;
				this.from = from.ToBitIndex();
				this.to = to.ToBitIndex();
				capturedPiece = board.mailBox[to.x][to.y]?.piece;
				pawnPromotion = false;
				promotedPiece = null;
				enpassantCapturedIndex = null;
				if (piece == PieceName.Pawn)
				{
					// Kiểm tra Pawn Promotion
					if ((color == Color.White && this.to >= 56)
						|| (color == Color.Black && this.to <= 7))
					{
						pawnPromotion = true;
						return;
					}

					// Kiểm tra Enpassant
					if (capturedPiece == null)
						enpassantCapturedIndex = color == Color.White ?
						(this.from + 7 == this.to ? this.from - 1 : this.from + 9 == this.to ? this.from + 1 : (int?)null)
						: (this.from - 7 == this.to ? this.from + 1 : this.from - 9 == this.to ? this.from - 1 : (int?)null);

					if (enpassantCapturedIndex != null)
					{
						var (x, y) = enpassantCapturedIndex.Value.ToMailBoxIndex();
						capturedPiece = board.mailBox[x][y].Value.piece;
					}
				}
			}


			public static async Task<MoveData?> New(Board board, (int x, int y) from, (int x, int y) to)
			{
				var data = new MoveData(board, from, to, out bool pawnPromotion);
				return !pawnPromotion ? data :
					(data.promotedPiece = await board.getPromotedPiece(data.color)) != null ?
					data : (MoveData?)null;
			}
		}

		private readonly Func<Color, Task<PieceName?>> getPromotedPiece;


		public void Move(MoveData data, bool isUndo)
		{
			ulong PIECE = color_piece_bitboards[(int)data.color][(int)data.piece];
			ulong OPPONENT_PIECE = data.capturedPiece != null ? color_piece_bitboards[(int)(data.color == Color.White ? Color.Black : Color.White)][(int)data.capturedPiece] : 0UL;
			var from = data.from.ToMailBoxIndex();
			var to = data.to.ToMailBoxIndex();

			if (!isUndo)
			{
				#region DO
				PIECE = PIECE.ClearBit(data.from);
				mailBox[from.x][from.y] = null;

				#region Đặt quân vào ô vị trí {to}
				mailBox[to.x][to.y] = (data.color, data.promotedPiece != null ? data.promotedPiece.Value : data.piece);
				if (data.promotedPiece != null)
				{
					var p = data.promotedPiece.Value;
					color_piece_bitboards[(int)data.color][(int)p] = color_piece_bitboards[(int)data.color][(int)p].SetBit(data.to);
				}
				else PIECE = PIECE.SetBit(data.to);
				#endregion

				#region Xử lý nếu có bắt quân đối phương
				if (data.capturedPiece != null)
					if (data.enpassantCapturedIndex != null)
					{
						// Bắt Tốt Qua Đường
						int bitIndex = data.enpassantCapturedIndex.Value;
						OPPONENT_PIECE = OPPONENT_PIECE.ClearBit(bitIndex);
						var (x, y) = bitIndex.ToMailBoxIndex();
						mailBox[x][y] = null;
					}
					else OPPONENT_PIECE = OPPONENT_PIECE.ClearBit(data.to);
				#endregion

				#region Xử lý trường hợp Nhập Thành (Castling)
				if (data.piece == PieceName.King)
				{
					ulong ROOK = color_piece_bitboards[(int)data.color][(int)PieceName.Rook];
					var ROOK_PIECE = (data.color, PieceName.Rook);
					if (data.color == Color.White)
					{
						if (data.to == 6)
						{
							#region Near Castling
							ROOK = ROOK.ClearBit(7);
							mailBox[7][0] = null;
							ROOK = ROOK.SetBit(5);
							mailBox[5][0] = ROOK_PIECE;
							#endregion
						}
						else if (data.to == 2)
						{
							#region Far Castling
							ROOK = ROOK.ClearBit(0);
							mailBox[0][0] = null;
							ROOK = ROOK.SetBit(3);
							mailBox[3][0] = ROOK_PIECE;
							#endregion
						}
					}
					else
					{
						if (data.to == 62)
						{
							#region Near Castling
							ROOK = ROOK.ClearBit(63);
							mailBox[7][7] = null;
							ROOK = ROOK.SetBit(61);
							mailBox[5][7] = ROOK_PIECE;
							#endregion
						}
						else if (data.to == 58)
						{
							#region Far Castling
							ROOK = ROOK.ClearBit(56);
							mailBox[0][7] = null;
							ROOK = ROOK.SetBit(59);
							mailBox[3][7] = ROOK_PIECE;
							#endregion
						}
					}
					color_piece_bitboards[(int)data.color][(int)PieceName.Rook] = ROOK;
				}

				#endregion
				#endregion
			}
			else
			{
				#region UNDO
				throw new NotImplementedException();
				#endregion
			}

			color_piece_bitboards[(int)data.color][(int)data.piece] = PIECE;
			if (data.capturedPiece != null) color_piece_bitboards[(int)(data.color == Color.White ? Color.Black : Color.White)][(int)data.capturedPiece] = OPPONENT_PIECE;
		}
		#endregion
	}
}