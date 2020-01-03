using System;


namespace GameHub.Cards.TienLen
{
	/// <summary>
	/// Nhóm các lá bài (Đôi, Ba/Sám Cô, Sảnh)
	/// </summary>
	public sealed class Set
	{
		private readonly Card[] cards;

		/// <summary>
		/// Lá bài mạnh nhất trong nhóm.
		/// </summary>
		private readonly Card maxCard;


		/// <summary>
		/// 2 (heo) > A > K > Q > J > 10 > 9 > 8 > 7 > 6 > 5 > 4 > 3
		/// </summary>
		private static readonly int[] NUMBER_RANK = new int[]
		{
			-1/*Invalid*/, 11/*A*/, 12/*2*/, 0/*3*/, 1/*4*/, 2/*5*/, 3/*6*/,
			4/*7*/, 5/*8*/, 6/*9*/, 7/*10*/, 8/*J*/, 9/*Q*/, 10/*K*/
		};

		/// <summary>
		/// ♥ Cơ > ♦ Rô > ♣ Nhép > ♠ Bích
		/// </summary>
		private static readonly int[] SUIT_RANK = new int[]
		{
			3/*♥*/, 2/*♦*/, 1/*♣*/, 0/*♠*/
		};

		/// <summary>
		/// <c>[(<see cref="int"/>)<see cref="Suit"/>]== color</c>
		/// </summary>
		private static readonly int[] SUIT_COLOR = new int[]
		{
			0, 0, 1, 1
		};


		private Set(Card maxCard, params Card[] cards)
		{
			this.maxCard = maxCard;
			this.cards = cards;
		}


		private static int Compare(Card a, Card b)
		{
			(int numRank, int suitRank) A = (NUMBER_RANK[(int)a.number], SUIT_RANK[(int)a.suit]);
			(int numRank, int suitRank) B = (NUMBER_RANK[(int)b.number], SUIT_RANK[(int)b.suit]);
			return A.numRank != B.numRank ? (A.numRank > B.numRank ? 1 : -1)
				: A.suitRank > B.suitRank ? 1 : -1;
		}
	}
}