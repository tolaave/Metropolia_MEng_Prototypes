using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Hex
{
	/// <summary>
	/// Cubic position container for hex coordinates
	/// </summary>
	public struct CubicPos
	{
		public int X { get; private set; }

		public int Y { get; private set; }

		public int Z { get; private set; }

		public CubicPos (int x, int y, int z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public static CubicPos operator + (CubicPos lhs, CubicPos rhs)
		{
			return new CubicPos (lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
		}

		public static CubicPos operator - (CubicPos lhs, CubicPos rhs)
		{
			return new CubicPos (lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
		}

		public static CubicPos operator * (CubicPos lhs, int rhs)
		{
			return new CubicPos (lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
		}

		public override string ToString ()
		{
			return string.Format ("[CubicPos: X={0}, Y={1}, Z={2}]", X, Y, Z);
		}

		public int DistanceTo(CubicPos other)
		{
			return (Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y) + Mathf.Abs(Z - other.Z)) / 2;
		}
	}
}