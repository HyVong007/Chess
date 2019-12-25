using System;
using System.Collections.Generic;


namespace Chess
{

	// Tương lai sửa thành class TurnManager






	/// <summary>
	/// Lưu lại lịch sử các <see cref="GameAction"/> được thực hiện khi người chơi chơi game và cho phép Undo/ Redo.
	/// <para>GameState chỉ được thay đổi thông qua Play/ Undo/ Redo.</para>
	/// </summary>
	/// <typeparam name="T">Kiểu dữ liệu của <see cref="GameAction.data"/> của từng loại game.</typeparam>
	public sealed class History<T> where T : struct
	{
		/// <summary>
		/// <c>&lt;<see cref="GameAction.data"/>, isUndo&gt;</c>: Thực hiện action để thay đổi Game State (Play/ Undo/ Redo).
		/// </summary>
		public event Action<T, bool> execute;


		/// <param name="execute"><c>&lt;<see cref="GameAction.data"/>, isUndo&gt;</c>: Thực hiện action để thay đổi Game State (Play/ Undo/ Redo).</param>
		/// <param name="maxActionCount">Số lượng action liên tiếp tối đa có thể lưu lại.<br/>
		/// Cũng chính là số bước Undo/Redo liên tiếp đối đa có thể thực hiện.</param>
		public History(byte maxActionCount = byte.MaxValue)
		{
			if (maxActionCount == 0) throw new ArgumentOutOfRangeException("MAX_ACTION_COUNT phải > 0 !");
			recentActions = new List<GameAction>(maxActionCount);
			undoneActions = new List<GameAction>(maxActionCount);
		}


		public struct GameAction
		{
			public int turn, playerID;
			public T data;
		}

		private readonly List<GameAction> recentActions, undoneActions;
		public int turn { get; private set; }

		public T? lastActionData => recentActions.Count > 0 ? recentActions[recentActions.Count - 1].data : (T?)null;


		public bool CanUndo(int playerID)
		{
			for (int i = recentActions.Count - 1; i >= 0; --i) if (recentActions[i].playerID == playerID) return true;
			return false;
		}


		public bool CanRedo(int playerID)
		{
			for (int i = 0, count = undoneActions.Count; i < count; ++i) if (undoneActions[i].playerID == playerID) return true;
			return false;
		}


		public void Undo(int playerID)
		{
			int id;
			GameAction action;

			do
			{
				--turn;
				action = recentActions[recentActions.Count - 1];
				recentActions.RemoveAt(recentActions.Count - 1);
				id = action.playerID;
				execute(action.data, true);
				if (undoneActions.Count == undoneActions.Capacity) undoneActions.RemoveAt(undoneActions.Count - 1);
				undoneActions.Insert(0, action);
			} while (id != playerID);
		}


		public void Redo(int playerID)
		{
			int id;
			do
			{
				var oldAction = undoneActions[0];
				undoneActions.RemoveAt(0);
				id = oldAction.playerID;
				execute(oldAction.data, false);
				int order = oldAction.turn - 1;
				while (recentActions.Count != 0)
				{
					var action = recentActions[recentActions.Count - 1];
					if (action.turn == order) break;
					execute(action.data, true);
					recentActions.RemoveAt(recentActions.Count - 1);
				}

				recentActions.Add(oldAction);
			} while (id != playerID);
		}


		/// <summary>
		/// Thực hiện action mới (không phải Undo/ Redo).
		/// </summary>
		public void Play(int playerID, T actionData)
		{
			execute(actionData, false);
			if (turn == int.MaxValue)
			{
				turn = 0;
				recentActions.Clear();
				undoneActions.Clear();
			}
			else ++turn;

			var action = new GameAction() { turn = turn, playerID = playerID, data = actionData };
			if (recentActions.Count == recentActions.Capacity)
			{
				if (undoneActions.Count > 0 && undoneActions[undoneActions.Count - 1].turn == recentActions[0].turn) undoneActions.RemoveAt(undoneActions.Count - 1);
				recentActions.RemoveAt(0);
			}
			recentActions.Add(action);
		}
	}
}