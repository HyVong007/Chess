namespace GameHub.Chess.GOChess
{
	public sealed class Piece
	{
		public readonly Color color;
		internal Land land;


		internal Piece(Color color, Land land)
		{
			this.color = color;
			this.land = land;
		}


		public override string ToString() => $"(color= {color}, land= {land})";
	}
}