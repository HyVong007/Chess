using System.Runtime.CompilerServices;
using System;


namespace GameHub.Cards.BaiCao
{
	/// <summary>
	/// Bộ 3 lá bài
	/// </summary>
	public readonly struct Tripple : IComparable<Tripple>
	{
		/// <summary>
		/// Nút của Bộ 3 lá
		/// </summary>
		private readonly int nut;

		/// <summary>
		/// Chứa cả 3 lá bài J, Q, K
		/// </summary>
		public readonly bool hasJQK;
		private const int MULTIPLY_JQK = (int)Number.J * (int)Number.Q * (int)Number.K;


		public Tripple(Card[] cards)
		{
			if (cards.Length != 3) throw new ArgumentOutOfRangeException("cards.Lentgh phải == 3 !");

			nut = 0;
			int mul = 1;
			for (int c = 0; c < 3; ++c)
			{
				var card = cards[c];
				nut += card;
				mul *= (int)card.number;
			}

			nut = (hasJQK = mul == MULTIPLY_JQK) ? 100 : nut % 10;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Tripple a, Tripple b) => a.nut == b.nut;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Tripple a, Tripple b) => a.nut != b.nut;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Tripple a, Tripple b) => a.nut > b.nut;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Tripple a, Tripple b) => a.nut < b.nut;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(Tripple a, Tripple b) => a.nut >= b.nut;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(Tripple a, Tripple b) => a.nut <= b.nut;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Tripple other) => nut > other.nut ? 1 : nut < other.nut ? -1 : 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(Tripple tripple) => tripple.nut;

		public override bool Equals(object _) => throw new NotImplementedException("Chưa biết có nên dùng hay không ?!");

		public override int GetHashCode() => throw new NotImplementedException("Chưa biết có nên dùng hay không ?!");
	}
}