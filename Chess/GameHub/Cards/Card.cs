using System;


namespace GameHub.Cards
{
	/// <summary>
	/// Chất
	/// </summary>
	public enum Suit
	{
		/// <summary>
		/// ♡ Cơ
		/// </summary>
		Heart = 0,

		/// <summary>
		/// ♢ Rô
		/// </summary>
		Diamond = 1,

		/// <summary>
		/// ♣ Chuồn (Nhép/ Tép)
		/// </summary>
		Tree = 2,

		/// <summary>
		/// ♠ Bích
		/// </summary>
		Spade = 3
	}



	/// <summary>
	/// Số (khác với Nút)
	/// </summary>
	public enum Number
	{
		/// <summary>
		/// Át/ Xì (Ace)
		/// </summary>
		A = 1,
		Num2 = 2, Num3 = 3, Num4 = 4, Num5 = 5, Num6 = 6, Num7 = 7, Num8 = 8, Num9 = 9, Num10 = 10,

		/// <summary>
		/// 🤠 Bồi (Jack)
		/// </summary>
		J = 11,

		/// <summary>
		/// ♕ Đầm (Queen)
		/// </summary>
		Q = 12,

		/// <summary>
		/// ♔ Già (King)
		/// </summary>
		K = 13
	}



	/// <summary>
	/// Lá bài thường trong bộ 52 lá tiêu chuẩn.
	/// <para>Độ mạnh mỗi lá và luật chơi chưa xác định.</para>
	/// </summary>
	public abstract class Card
	{
		public readonly Number number;
		public readonly Suit suit;

		/// <summary>
		/// Nút của lá bài.
		/// </summary>
		public abstract int Nut { get; }


		protected Card(Number number, Suit suit)
		{
			this.number = number;
			this.suit = suit;
		}


		public override string ToString() => $"Card: number= {number}, suit= {suit}";

		public override bool Equals(object _) => throw new NotImplementedException("Abstract Card: Chưa biết có nên dùng hay không ?!");

		public override int GetHashCode() => throw new NotImplementedException("Abstract Card: Chưa biết có nên dùng hay không ?!");
	}



	/// <summary>
	/// 🃏 2 lá bài đặc biệt: Phăng-teo.
	/// </summary>
	public readonly struct Joker
	{
		public readonly bool red;


		public Joker(bool red) => this.red = red;


		public override string ToString() => $"Joker: {(red ? "RED" : "BLACK")}";
	}
}