using System;
using Chess;
using Chess.ChineseChess;


namespace Chess.ChineseChess.Tests
{
	public class Test
	{
		static void Main()
		{
			var a = Board.CloneDefaultMailBox;


			#region Input
			a[0][0] = new Piece() { color = Color.Red, name = PieceName.Cannon, hidden = true };
			a[8][0] = new Piece() { color = Color.Red, name = PieceName.Cannon, hidden = true };
			a[1][2] = new Piece() { color = Color.Red, name = PieceName.Rook, hidden = true };
			a[7][2] = new Piece() { color = Color.Red, name = PieceName.Rook, hidden = true };
			#endregion


			var history = new History<Board.MoveData>();
			var board = Board.NewCustom(history, a);
			board.Print();
		}
	}
}