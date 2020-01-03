using System;


namespace GameHub.Cards.Bai3Cay
{
	/// <summary>
	/// <para>BÀI 3 CÂY (ĐẾM NÚT MIỀN BẮC)</para>
	/// Các lá bài : A đến 9 tương ứng Nút = 1 đến 9<br/>
	/// Độ mạnh Chất: Rô ♦ > Cơ ♥ > Chuồn ♣ > Bích ♠<br/>
	/// A Rô ♦ có điểm lớn nhất (mạnh nhất)<br/>
	/// Số nút và số điểm của 1 lá bài là khác nhau !<br/>
	/// Nếu <c>{Tổng nút 3 lá} == 20 thì {Tổng nút cuối cùng} = 10</c><br/>
	/// Nếu không thì tính: <c>{Tổng nút cuối cùng} = {Tổng nút 3 lá} % 10</c>
	/// </summary>
	public sealed class Card : Cards.Card
	{
		/// <summary>
		/// Nút của lá bài.
		/// </summary>
		private readonly int nut;


		public Card(Number number, Suit suit) : base(number, suit) =>
			nut = (int)number >= 10 ? throw new ArgumentException("Bài 3 Cây không dùng các lá bài: 10, J, Q, K !")
			: (int)number;


		public override int Nut => nut;
	}
}
