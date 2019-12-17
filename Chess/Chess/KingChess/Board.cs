using System;
using System.Collections.Generic;


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
		#region Khai báo dữ liệu và khởi tạo
		/// <summary>
		/// <c>[(<see cref="int"/>)<see cref="Color"/>][(<see cref="int"/>)<see cref="PieceName"/>]= bitboard</c><br/>
		/// Nhớ update <see cref="mailBox"/> sau khi kết thúc modify
		/// </summary>
		private readonly ulong[][] color_piece_bitboards;

		private readonly (Color color, PieceName piece)?[][] mailBox;

		public static readonly (Color color, PieceName piece)?[][] DEFAULT_MAILBOX = new (Color color, PieceName piece)?[][]
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



		public Board((Color color, PieceName piece)?[][] mailBox)
		{
			this.mailBox = mailBox;

			#region Khởi tạo {color_piece_bitboards}
			color_piece_bitboards = new ulong[2][];
			int piece_length = Enum.GetValues(typeof(PieceName)).Length;
			foreach (Color color in Enum.GetValues(typeof(Color))) color_piece_bitboards[(int)color] = new ulong[piece_length];
			for (int ROW = mailBox.Length, COL = mailBox[0].Length, y = 0, index = 0; y < COL; ++y)
				for (int x = 0; x < ROW; ++x, ++index)
				{
					var data = mailBox[x][y];
					if (data == null) continue;
					color_piece_bitboards[(int)data.Value.color][(int)data.Value.piece] |= 1UL << index;
				}
			#endregion
		}
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
						Console.ForegroundColor = d.Value.color == Color.White ? ConsoleColor.White : ConsoleColor.Black;
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


		#region Find Pseudo-Legal Moves
		public const ulong
			CENTER = 0x1818000000UL,
			EXTENDED_CENTER = 0x3C3C3C3C0000UL,
			FILE_A = 0x101010101010101UL,
			FILE_H = 0x8080808080808080UL,
			RANK_1 = 0xFFUL,
			RANK_4 = 0xFF000000UL,
			RANK_5 = 0xFF00000000UL,
			RANK_8 = 0xFF00000000000000UL;


		/// <summary>
		/// Tìm bitboard tập hợp các ô có thể move của các quân cờ trong bitboard [color][piece]. Chưa kiểm tra xem King có bị chiếu.
		/// <para>bit 1 = ô đi tới được (move-square)</para>
		/// </summary>
		public ulong FindPseudoLegalMoves(Color color, PieceName piece)
		{
			ulong result = 0UL;
			ulong EMPTY = empty;
			// Performance: quân trắng cần sử dụng Tập hợp quân đen
			ulong BLACK_PIECES = color == Color.White ? this[Color.Black] : 0UL;
			// Performance: quân đen cần sử dụng Tập hợp quân trắng
			ulong WHITE_PIECES = color == Color.Black ? this[Color.White] : 0UL;


			switch (piece)
			{
				case PieceName.Pawn:
					#region Pawn Moves
					ulong P = color_piece_bitboards[(int)color][(int)piece];
					if (color == Color.White)
					{
						#region White Pawn Moves: dịch chuyển bằng Left Shift <<
						ulong P_ls = P << 7;
						result |= P_ls & BLACK_PIECES & ~FILE_H;     // ăn chéo trái
						P_ls <<= 1;
						ulong forward1 = P_ls & EMPTY;              // đi về trước 1 bước
						result |= forward1;
						result |= forward1 << 8 & EMPTY & RANK_4;   // đi về trước 2 bước
						P_ls <<= 1;
						result |= P_ls & BLACK_PIECES & ~FILE_A;     // ăn chéo phải		

						// enpassant (Bắt Tốt qua đường) ?
						#endregion
					}
					else
					{
						#region Black Pawn Moves: dịch chuyển bằng Right Shift >>
						ulong P_rs = P >> 7;
						result |= P_rs & WHITE_PIECES & ~FILE_A;     // ăn chéo trái
						P_rs >>= 1;
						ulong forward1 = P_rs & EMPTY;              // đi về trước 1 bước
						result |= forward1;
						result |= forward1 >> 8 & EMPTY & RANK_5;   // đi về trước 2 bước
						P_rs >>= 1;
						result |= P_rs & WHITE_PIECES & ~FILE_H;     // ăn chéo phải		

						// enpassant (Bắt Tốt qua đường) ?
						#endregion
					}
					return result;

				#endregion


				case PieceName.Rook:
				#region Rook Moves










				#endregion









				default: return result;
			}
		}







		#endregion
	}
}