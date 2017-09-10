namespace Utilities.Hex
{
	/// <summary>
	/// Axial position container for hex coordinates
	/// </summary>
	public struct AxialPos
	{
		public int X { get; private set; }

		public int Y { get; private set; }

		public AxialPos (int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public override bool Equals (object obj)
		{
			return obj is AxialPos && this == (AxialPos)obj;
		}

		public override int GetHashCode ()
		{
			return (X.GetHashCode () << 8) ^ (Y.GetHashCode ());
		}

		public static bool operator == (AxialPos lhs, AxialPos rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator != (AxialPos lhs, AxialPos rhs)
		{
			return !(lhs == rhs);
		}

		public override string ToString ()
		{
			return string.Format ("[AxialPos: X={0}, Y={1}]", X, Y);
		}
	}
}