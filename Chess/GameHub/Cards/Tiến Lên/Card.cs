using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;


namespace GameHub.Cards.TienLen
{
	/// <summary>
	/// <para>BÀI TIẾN LÊN</para>
	/// Độ mạnh của lá bài: 2 (heo) > A > K > Q > J > 10 > 9 > 8 > 7 > 6 > 5 > 4 > 3<br/>
	/// ♥ Cơ > ♦ Rô > ♣ Nhép > ♠ Bích
	/// </summary>
	public sealed class Card : Cards.Card
	{
		public override int Nut => throw new InvalidOperationException("Bài Tiến Lên không dùng Nút !");


		public Card(Number number, Suit suit) : base(number, suit) { }
	}
}