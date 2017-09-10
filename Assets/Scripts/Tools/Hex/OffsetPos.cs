namespace Utilities.Hex
{
	/// <summary>
	/// Offset position container for hex coordinates
	/// </summary>
	public struct OffsetPos
	{
		public int X { get; private set; }

		public int Y { get; private set; }

		public OffsetPos (int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public override bool Equals (object obj)
		{
			return obj is OffsetPos && this == (OffsetPos)obj;
		}

		public override int GetHashCode ()
		{
			return (X.GetHashCode () << 8) ^ (Y.GetHashCode ());
		}

		public static bool operator == (OffsetPos lhs, OffsetPos rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator != (OffsetPos lhs, OffsetPos rhs)
		{
			return !(lhs == rhs);
		}

		public override string ToString ()
		{
			return string.Format ("[OffsetPos: X={0}, Y={1}]", X, Y);
		}

		public bool InRange(float x1, float y1, float x2, float y2)
		{
			return (X >= x1) && (Y >= y1) && (X <= x2) && (Y <= y2);
		}
	}
}