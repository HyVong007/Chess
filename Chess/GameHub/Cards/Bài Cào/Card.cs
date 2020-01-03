using System.Runtime.CompilerServices;


namespace GameHub.Cards.BaiCao
{
	/// <summary>
	/// <para>BÀI CÀO (ĐẾM NÚT MIỀN NAM)</para>
	/// <see cref="Number"/>= (A, 2, 3, 4, 5, 6, 7, 8, 9, 10) => <c>Nút mỗi lá = (<see cref="int"/>)<see cref="Number"/></c><br/>
	/// <see cref="Number"/>= (J, Q, K) => <c>Nút mỗi lá = 10</c><br/>
	/// Không phụ thuộc <see cref="Suit"/><br/>
	/// Nếu sở hữu cả 3 lá J, Q, K &lt;=&gt; thì thắng ngay ("Ba Cào", "Ba Tiên")<br/>
	/// Nếu không thì tính: <c>{Tổng nút cuối cùng} = {Tổng nút 3 lá} % 10</c><br/>
	/// Ai có {Tổng nút cuối cùng} lớn nhất thì thắng và lấy hết tiền cược.<br/>
	/// Nếu nhiều người có {Tổng nút cuối cùng} bằng nhau thì tiền cược chia đều hoặc chơi ván phụ.
	/// </summary>
	public sealed class Card : Cards.Card
	{
		/// <summary>
		/// Nút của lá bài
		/// </summary>
		private readonly int nut;

		/// <summary>
		/// <c>[(<see cref="int"/>)<see cref="Cards.Card.number"/>]==<see cref="score"/></c>
		/// </summary>
		private static readonly int[] NUMBER_TO_NUT = new int[]
		{
			-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10
		};


		public Card(Number number, Suit suit) : base(number, suit) => nut = NUMBER_TO_NUT[(int)number];


		public override int Nut => nut;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(Card card) => card.nut;
	}
}