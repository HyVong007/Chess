using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Chess
{
	public static class Number_Extension
	{
		/// <summary>
		///  <c>_1_LS[index] == 1UL &lt;&lt; index</c>
		/// </summary>
		private static readonly ulong[] _1_LS = new ulong[64];

		/// <summary>
		/// NOT_1_LS[index] == ~<see cref="_1_LS"/>
		/// </summary>
		private static readonly ulong[] NOT_1_LS = new ulong[64];


		static Number_Extension()
		{
			for (int i = 0; i < 64; ++i) NOT_1_LS[i] = ~(_1_LS[i] = 1UL << i);
		}


		/// <summary>
		/// Return bit[index]. Other bits will be zero.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong GetBit(this ulong u, int index) => u & _1_LS[index];


		/// <summary>
		/// Bit[index] is changed to 1. Other bits will be preserved.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong SetBit(this ulong u, int index) => u | _1_LS[index];


		/// <summary>
		/// Bit[index] is changed to 0. Other bits will be preserved.
		/// </summary>
		/// <param name="u"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong ClearBit(this ulong u, int index) => u & NOT_1_LS[index];


		/// <summary>
		/// Number of bits which is set (bit 1)
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
		/// Nghịch đảo chuỗi bit trong đoạn [startIndex, stopIndex]
		/// <para>Chú ý: index của chuỗi bit ngược với thứ tự của Integral Litteral (Integral Litteral cùng thứ tự với <see cref="string"/> đại diện) !</para>
		/// </summary>
		/// <param name="startIndex">Vị trí bắt đầu (Inclusive)</param>
		/// <param name="stopIndex">Vị trí kết thúc (Inclusive)</param>
		public static ulong ReverseBit(this ulong source, int startIndex, int stopIndex)
		{
			ulong tmp = source;

			#region tmp: xóa những bit ngoài [startIndex, stopIndex]; source: xóa những bit trong [startIndex, stopIndex]
			ulong mask = 0UL;
			for (int i = startIndex; i <= stopIndex; ++i) mask |= _1_LS[i];     // 0...0[1...1]0...0
			tmp &= mask;
			mask = ~mask;                                                       // 1...1[0...0]1...1
			source &= mask;
			#endregion

			ulong reversed = 0UL;

			#region Lấy bit từ tmp và nghịch đảo vị trí rồi đưa vào result
			int length = stopIndex - startIndex + 1;
			int STEP = length / 2;
			if (length % 2 != 0) reversed |= tmp & _1_LS[startIndex + STEP];

			for (int i = 0, L = stopIndex, R = startIndex; i < STEP; ++i, --L, ++R)
			{
				int distance = L - R;
				reversed |= (tmp & _1_LS[L]) >> distance;
				reversed |= (tmp & _1_LS[R]) << distance;
			}
			#endregion

			return source | reversed;
		}


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
	}



	public struct ReadOnlyArray<T> : IEnumerable<T>
	{
		private readonly T[] array;


		public ReadOnlyArray(T[] array) => this.array = array;


		public T this[int index] => array[index];

		public int Length => array.Length;

		public IEnumerator<T> GetEnumerator() => (array as IEnumerable<T>).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => array.GetEnumerator();
	}
}