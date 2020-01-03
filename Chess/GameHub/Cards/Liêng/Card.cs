using System;


namespace GameHub.Cards.Lieng
{
	public sealed class Card : Cards.Card
	{
		public override int Nut => throw new InvalidOperationException();


		public Card(Number number, Suit suit) : base(number, suit) { }
	}
}