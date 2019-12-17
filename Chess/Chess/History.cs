//
// LinkedList<T> rất chậm so với List<T>, và chậm hơn nữa với T[].
// Cần tối ưu
//
//


using System;
using System.Collections.Generic;


namespace Chess
{
	/// <summary>
	/// Lưu lại lịch sử các <see cref="GameAction"/> được thực hiện khi người chơi chơi game và cho phép Undo/ Redo.
	/// <para>GameState chỉ được thay đổi thông qua Play/ Undo/ Redo.</para>
	/// </summary>
	public sealed class History<T> where T : struct
	{
		private readonly int MAX_ACTION_COUNT;

		/// <summary>
		/// <c>&lt;<see cref="GameAction.data"/>, isUndo&gt;</c>: Thực hiện action để thay đổi Game State (Play/ Undo/ Redo).
		/// </summary>
		private readonly Action<T, bool> execute;


		/// <param name="execute"><c>&lt;<see cref="GameAction.data"/>, isUndo&gt;</c>: Thực hiện action để thay đổi Game State (Play/ Undo/ Redo).</param>
		/// <param name="maxActionCount">Số lượng action liên tiếp tối đa có thể lưu lại.<br/>
		/// Cũng chính là số bước Undo/Redo liên tiếp đối đa có thể thực hiện.</param>
		public History(Action<T, bool> execute, int maxActionCount = 100)
		{
			this.execute = execute;
			MAX_ACTION_COUNT = maxActionCount;
		}


		public struct GameAction
		{
			public int turn, playerID;
			public T data;
		}

		// Nên thay bằng array 1 chiều hoặc List<T>
		private readonly LinkedList<GameAction> recentActions = new LinkedList<GameAction>(), undoneActions = new LinkedList<GameAction>();
		public int turn { get; private set; }

		/// <summary>
		/// Hành động gần nhất được thực hiện.
		/// </summary>
		public GameAction lastAction => recentActions.Last.Value;


		public bool CanUndo(int playerID)
		{
			var node = recentActions.Last;
			while (node != null)
			{
				if (node.Value.playerID == playerID) return true;
				node = node.Previous;
			}
			return false;
		}


		public bool CanRedo(int playerID)
		{
			foreach (var data in undoneActions) if (data.playerID == playerID) return true;
			return false;
		}


		public void Undo(int playerID)
		{
			int id;
			LinkedListNode<GameAction> node;
			do
			{
				--turn;
				node = recentActions.Last;
				recentActions.RemoveLast();
				var value = node.Value;
				id = value.playerID;
				execute(value.data, true);
				undoneActions.AddFirst(node);
				if (undoneActions.Count > MAX_ACTION_COUNT) undoneActions.RemoveLast();
			} while (id != playerID);
		}


		public void Redo(int playerID)
		{
			int id;
			do
			{
				var nodeUndo = undoneActions.First;
				undoneActions.RemoveFirst();
				var v = nodeUndo.Value;
				id = v.playerID;
				execute(v.data, false);
				int order = nodeUndo.Value.turn - 1;
				while (recentActions.Count != 0)
				{
					var value = recentActions.Last.Value;
					if (value.turn == order) break;
					execute(value.data, true);
					recentActions.RemoveLast();
				}

				recentActions.AddLast(nodeUndo);
			} while (id != playerID);
		}


		/// <summary>
		/// Thực hiện action mới (không phải Undo/ Redo).
		/// </summary>
		public void Play(int playerID, T actionData)
		{
			execute(actionData, false);
			var action = new GameAction() { turn = turn++, playerID = playerID, data = actionData };
			recentActions.AddLast(action);
			if (recentActions.Count > MAX_ACTION_COUNT)
			{
				if (recentActions.First.Value.turn == undoneActions.Last?.Value.turn) undoneActions.RemoveLast();
				recentActions.RemoveFirst();
			}
		}
	}
}