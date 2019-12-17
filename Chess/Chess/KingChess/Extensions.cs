using System;


namespace Chess.KingChess
{
	public static class Number_Extension
	{
		#region Thao tác bit
		/// <summary>
		/// [index] = 1
		/// </summary>
		public static void SetBit(ref this ulong u, int index) => u |= (1UL << index);

		/// <summary>
		/// [index] = 0
		/// </summary>
		public static void ClearBit(ref this ulong u, int index) => u &= ~(1UL << index);

		/// <summary>
		/// [index] != 0 ?
		/// </summary>
		public static bool IsBitSet(this ulong u, int index) => (u & (1UL << index)) != 0;

		/// <summary>
		/// Number of bits which is set.
		/// </summary>
		public static int BitSetCount(this ulong u)
		{
			int count = 0;
			while (u != 0)
			{
				++count;
				u &= u - 1;
			}
			return count;
		}

		/// <summary>
		/// Rotate bits-circle to left/right.<br/>
		/// </summary>
		/// <param name="step"><c>if (step &gt;= 0): left shift<br/> 
		/// else: right shift</c></param>
		public static void CircularShiftBit(ref this ulong u, int step) => u = step >= 0 ? u << step | u >> (64 - step) : u >> -step | u << (64 + step);


		/// <summary>
		/// Nghịch đảo chuỗi bit trong đoạn [startIndex, stopIndex]
		/// <para>Chú ý: index của chuỗi bit ngược với thứ tự của Integral Litteral (Integral Litteral cùng thứ tự với <see cref="string"/> đại diện) !</para>
		/// </summary>
		/// <param name="startIndex">Vị trí bắt đầu (Inclusive)</param>
		/// <param name="stopIndex">Vị trí kết thúc (Inclusive)</param>
		public static void Reverse(ref this ulong source, int startIndex, int stopIndex)
		{
			ulong tmp = source;

			#region tmp: xóa những bit ngoài [startIndex, stopIndex]; source: xóa những bit trong [startIndex, stopIndex]
			ulong mask = 0UL;
			for (int i = startIndex; i <= stopIndex; ++i) mask |= 1UL << i;     // 0...0[1...1]0...0
			tmp &= mask;
			mask = ~mask;                                                       // 1...1[0...0]1...1
			source &= mask;
			#endregion

			ulong result = 0UL;

			#region Lấy bit từ tmp và nghịch đảo vị trí rồi đưa vào result
			int length = stopIndex - startIndex + 1;
			int STEP = length / 2;
			if (length % 2 != 0) result |= tmp & (1UL << (startIndex + STEP));

			for (int i = 0, L = stopIndex, R = startIndex; i < STEP; ++i, --L, ++R)
			{
				int distance = L - R;
				result |= (tmp & (1UL << L)) >> distance;
				result |= (tmp & (1UL << R)) << distance;
			}
			#endregion

			source |= result;
		}
		#endregion


		#region DEBUG: Xuất string dạng binary: ulong
		/// <summary>
		/// Return: <see cref="string"/> dạng binary.
		/// </summary>
		public static string ToBinary(this ulong u)
		{
			char[] chars = new char[64];
			for (int x = 0; x < 64; ++x) chars[x] = '0';
			char[] INT_CHAR = new char[] { '0', '1' };
			int index = 0;
			while (u != 0)
			{
				chars[index++] = INT_CHAR[u % 2];
				u /= 2;
			}
			Array.Reverse(chars);
			return new string(chars);
		}


		/// <summary>
		/// In ra binary <see cref="string"/> với màu sắc: bit 1 là màu đỏ
		/// </summary>
		public static void PrintColorBinary(this ulong u)
		{
			string bin = ToBinary(u);
			for (int i = 0; i < bin.Length; ++i)
			{
				char c = bin[i];
				Console.ForegroundColor = c == '1' ? ConsoleColor.Red : ConsoleColor.White;
				Console.Write(c);
				if (i < bin.Length - 1 && (i + 1) % 4 == 0) Console.Write('_');
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();
		}


		/// <summary>
		/// Return: <see cref="string"/> dạng binary trong bàn cờ Vua 8x8.
		/// </summary>
		public static string ToBinary8x8(this ulong u)
		{
			string bin = ToBinary(u);
			string result = "";
			for (int y = 7, start = 7; y >= 0; --y, start += 8)
			{
				result += $"{y + 1}  ";
				for (int x = 0; x < 8; ++x) result += $"  {bin[start - x]}  ";
				result += "\n\n";
			}
			return result + "\n     A    B    C    D    E    F    G    H    \n";
		}


		/// <summary>
		/// In ra nhị phân dạng bàn cờ Vua 8x8 có màu sắc. bit 1 = màu đỏ
		/// </summary>
		public static void PrintColorBinary8x8(this ulong u)
		{
			string bin = ToBinary(u);
			for (int y = 7, start = 7; y >= 0; --y, start += 8)
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write($"{y + 1}  ");
				for (int x = 0; x < 8; ++x)
				{
					char c = bin[start - x];
					Console.ForegroundColor = c == '1' ? ConsoleColor.Red : ConsoleColor.White;
					Console.Write($"  {c}  ");
				}
				Console.Write("\n\n");
			}
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("\n     A    B    C    D    E    F    G    H    ");
			Console.ForegroundColor = ConsoleColor.White;
		}
		#endregion
	}
}