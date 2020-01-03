using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace GameHub
{
	public readonly struct Rect
	{
		public readonly int xMin, yMin, xMax, yMax;


		public Rect(int xMin, int yMin, int xMax, int yMax)
		{
			this.xMin = xMin; this.yMin = yMin; this.xMax = xMax; this.yMax = yMax;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(int x, int y) => xMin <= x && x <= xMax && yMin <= y && y <= yMax;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains((int x, int y) index) => Contains(index.x, index.y);


		public override string ToString() => $"(xMin= {xMin}, yMin= {yMin}, xMax= {xMax}, yMax= {yMax})";
	}



	public readonly struct ReadOnlyArray<T> : IEnumerable<T>
	{
		private readonly T[] array;


		public ReadOnlyArray(T[] array) => this.array = array;


		public T this[int index] => array[index];

		public int Length => array.Length;

		public IEnumerator<T> GetEnumerator() => (array as IEnumerable<T>).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => array.GetEnumerator();
	}
}