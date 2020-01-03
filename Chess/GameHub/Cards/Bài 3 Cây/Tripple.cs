using System.Runtime.CompilerServices;
using System;


namespace GameHub.Cards.Bai3Cay
{
	public readonly struct Tripple : IComparable<Tripple>
	{
		/// <summary>
		/// Nút của Bộ 3 lá
		/// </summary>
		public readonly int nut;

		private readonly int maxSuitRank;
		private readonly bool has_A_Diamond;

		/// <summary>
		/// Độ mạnh của Chất: Rô ♦ > Cơ ♥ > Chuồn ♣ > Bích ♠
		/// <para><c>[(<see cref="int"/>)<see cref="Suit"/>]==rank</c></para>
		/// </summary>
		private static readonly int[] SUIT_RANK = new int[]
		{
			2,	// Suit.Heart ♡
			3,	// Suit.Diamond ♢
			1,	// Suit.Tree ♣
			0	// Suit.Spade ♠
		};


		public Tripple(Card[] cards)
		{
			if (cards.Length != 3) throw new ArgumentOutOfRangeException("cards.Lentgh phải == 3 !");

			nut = 0;
			maxSuitRank = 0;
			has_A_Diamond = false;
			for (int c = 0; c < 3; ++c)
			{
				var card = cards[c];
				nut += card.Nut;
				if (card.number == Number.A && card.suit == Suit.Diamond) has_A_Diamond = true;
				maxSuitRank = Math.Max(maxSuitRank, SUIT_RANK[(int)card.suit]);
			}

			nut = has_A_Diamond ? 100 : nut == 20 ? 10 : nut <= 10 ? nut : nut % 10;
		}


		public int CompareTo(Tripple other) =>
			nut != other.nut ? (nut > other.nut ? 1 : -1)
			: has_A_Diamond ? 1
			: other.has_A_Diamond ? -1
			: (maxSuitRank > other.maxSuitRank ? 1 : -1);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Tripple a, Tripple b) => a.CompareTo(b) > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Tripple a, Tripple b) => a.CompareTo(b) < 0;

		public static bool operator ==(Tripple _, Tripple __) => throw new InvalidOperationException("Bài 3 cây: 2 Bộ luôn có độ mạnh khác nhau !");

		public static bool operator !=(Tripple _, Tripple __) => throw new InvalidOperationException("Bài 3 cây: 2 Bộ luôn có độ mạnh khác nhau !");

		public override bool Equals(object _) => throw new NotImplementedException("Chưa biết có nên dùng hay không ?!");

		public override int GetHashCode() => throw new NotImplementedException("Chưa biết có nên dùng hay không ?!");
	}
}