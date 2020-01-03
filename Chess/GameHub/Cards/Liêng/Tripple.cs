using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace GameHub.Cards.Lieng
{
	/// <summary>
	/// Bộ 3 lá bài: Sáp, Liêng, Ảnh, Điểm
	/// </summary>
	public readonly struct Tripple : IComparable<Tripple>
	{
		public enum Type
		{
			Sap = 3, Lieng = 2, Anh = 1, Diem = 0
		}
		public readonly Type type;


		#region Dữ liệu để so sánh {type}
		/// <summary>
		/// Điểm
		/// </summary>
		private readonly int score;

		private readonly int numberRank;

		/// <summary>
		/// Liêng, Ảnh, Điểm
		/// </summary>
		private readonly int suitRank;

		/// <summary>
		/// Liêng, Ảnh, Điểm
		/// </summary>
		private static readonly int[] SUIT_RANK = new int[]
		{
			2/*♡*/, 3/*♢*/, 1/*♣*/, 0/*♠*/
		};

		/// <summary>
		/// Sáp, Ảnh
		/// </summary>
		private static readonly int[] NUMBER_RANK = new int[]
		{
			-1/*Invalid*/, 12/*A*/, 0/*2*/, 1/*3*/, 2/*4*/, 3/*5*/, 4/*6*/,
			5/*7*/, 6/*8*/, 7/*9*/, 8/*10*/, 9/*J*/, 10/*Q*/, 11/*K*/
		};

		/// <summary>
		/// Điểm
		/// </summary>
		private static readonly int[] NUMBER_DIEM = new int[]
		{
			-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 0, 0
		};

		private static readonly (Number a, Number b, Number c)[] LIENG_TRIPPLE = new (Number a, Number b, Number c)[]
		{
			(Number.A, Number.Num2, Number.Num3),		// Liêng nhỏ nhất
			(Number.Num2, Number.Num3, Number.Num4),
			(Number.Num3, Number.Num4, Number.Num5),
			(Number.Num4, Number.Num5, Number.Num6),
			(Number.Num5, Number.Num6, Number.Num7),
			(Number.Num6, Number.Num7, Number.Num8),
			(Number.Num7, Number.Num8, Number.Num9),
			(Number.Num8, Number.Num9, Number.Num10),
			(Number.Num9, Number.Num10, Number.J),
			(Number.Num10, Number.J, Number.Q),
			(Number.J, Number.Q, Number.K),
			(Number.A, Number.Q, Number.K)				// Liêng lớn nhất
		};

		private static readonly List<Number> list = new List<Number>(3);
		#endregion


		public Tripple(Card[] cards)
		{
			if (cards.Length != 3) throw new ArgumentOutOfRangeException();

			score = numberRank = suitRank = 0;
			bool sap = true;
			var num = cards[0].number;
			bool anh = true;
			list.Clear();
			for (int c = 0; c < 3; ++c)
			{
				var card = cards[c];
				if (card.number != num) sap = false;
				if ((int)card.number <= 10) anh = false;
				list.Add(card.number);
				int rank = SUIT_RANK[(int)card.suit];
				if (rank > suitRank) suitRank = rank;
			}

			#region Sáp ?
			if (sap)
			{
				type = Type.Sap;
				numberRank = NUMBER_RANK[(int)num];
				return;
			}
			#endregion

			#region Liêng ?
			list.Sort();
			var tripple = (list[0], list[1], list[2]);
			for (int t = 0; t < LIENG_TRIPPLE.Length; ++t)
				if (tripple == LIENG_TRIPPLE[t])
				{
					type = Type.Lieng;
					numberRank = t;
					return;
				}
			#endregion

			#region Ảnh ?
			if (anh)
			{
				type = Type.Anh;
				for (int c = 0; c < 3; ++c)
					if ((int)cards[c].number > numberRank) numberRank = (int)cards[c].number;
				return;
			}
			#endregion

			#region Điểm !
			type = Type.Diem;
			for (int c = 0; c < 3; ++c)
			{
				int i = (int)cards[c].number;
				score += NUMBER_DIEM[i];
				int rank = NUMBER_RANK[i];
				if (rank > numberRank) numberRank = rank;
			}
			score %= 10;
			#endregion
		}


		public int CompareTo(Tripple other)
		{
			if (type != other.type) return (int)type > (int)other.type ? 1 : -1;

			switch (type)
			{
				case Type.Sap:
					return numberRank > other.numberRank ? 1 : -1;

				case Type.Lieng:
					return numberRank > other.numberRank ? 1 : numberRank < other.numberRank ? -1
						: suitRank > other.suitRank ? 1 : -1;

				case Type.Anh:
					return suitRank > other.suitRank ? 1 : suitRank < other.suitRank ? -1
						: numberRank > other.numberRank ? 1 : -1;

				case Type.Diem:
					return score > other.score ? 1 : score < other.score ? -1
						: suitRank > other.suitRank ? 1 : suitRank < other.suitRank ? -1
						: numberRank > other.numberRank ? 1 : -1;
			}
			throw new Exception();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Tripple a, Tripple b) => a.CompareTo(b) > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Tripple a, Tripple b) => a.CompareTo(b) < 0;


		public static bool operator ==(Tripple a, Tripple b) => throw new InvalidOperationException();
		public static bool operator !=(Tripple a, Tripple b) => throw new InvalidOperationException();
		public override bool Equals(object _) => throw new NotImplementedException("Chưa biết có nên dùng hay không ?!");
		public override int GetHashCode() => throw new NotImplementedException("Chưa biết có nên dùng hay không ?!");
	}
}