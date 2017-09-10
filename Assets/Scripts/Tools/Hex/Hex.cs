using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Hex
{
	/// <summary>
	/// Functionality for handling hex grid coordinates
	/// 
	/// This struct represents a single coordinate in hexagonal grid space, with three
	/// possible presentations of its state:
	/// 
	/// Cube:
	/// 
	///     XYZ    	->         0,1,-1
	/// XYZ     XYZ	->  -1,1,0        1,0,-1
	///     XYZ		->         0,0,0
	/// XYZ     XYZ	->  -1,0,1        1,-1,0
	///     XYZ		->         0,-1,1
	/// 
	/// Axial:
	/// 
	/// XY       	->  0,0
	///    XY    	->      1,0
	/// XY    XY    ->  0,1     2,0
	///    XY    XY	-> 	    1,1     3,0
	/// XY    XY	->	0,2     2,1
	///      
	/// Offset:
	/// 
	/// XY    XY	->  0,0     2,0
	///    XY    XY	->      1,0     3,0
	/// XY    XY	->  0,1     2,1
	///    XY    XY	-> 	    1,1     3,1
	/// 
	/// The canical coordinate is kept in memory as axial coordinate to save memory,
	/// calculations are performed in cube space, and offset coordinates can be used
	/// to optimize storage of map data which usually is presented as interleaved hex
	/// grid.
	/// </summary>
	public struct Hex : IEquatable<Hex>
	{
		public enum Side
		{
			TopLeft = 0,
			Top = 1,
			TopRight = 2,
			BottomRight = 3,
			Bottom = 4,
			BottomLeft = 5,
			NumSides = 6
		}

		/// <summary>
		/// Shortcuts for the six neighbors of hex coordinates
		/// </summary>
		static public readonly Dictionary<Side,CubicPos> directions = new Dictionary<Side,CubicPos>()
		{
			{
				Side.TopLeft,
				new CubicPos(-1, 1, 0)
			},
			{
				Side.Top,
				new CubicPos(0, 1, -1)
			},
			{
				Side.TopRight,
				new CubicPos(1, 0, -1)
			},
			{
                Side.BottomRight,
                new CubicPos(1, -1, 0)
            },
            {
                Side.Bottom,
                new CubicPos(0, -1, 1)
            },
            {
                Side.BottomLeft,
                new CubicPos(-1, 0, 1)
            }
        };

        public const float Sqrt3 = 1.7320508076f;

        private AxialPos canonical;

        public CubicPos Cube
        {
            get
            {
                return new CubicPos(
                    canonical.X,
                    -canonical.X - canonical.Y,
                    canonical.Y
                );
            }
            set
            {
                canonical = new AxialPos(value.X, value.Z);
            }
        }

        public AxialPos Axial
        {
            get
            {
                return canonical;
            }
            set
            {
                canonical = value;
            }
        }

        public OffsetPos Offset
        {
            get
            {
                return new OffsetPos(
                    canonical.X,
                    canonical.Y + (canonical.X / 2)
                );
            }
            set
            {
                canonical = new AxialPos(
                    value.X,
                    value.Y - (value.X / 2)
                );
            }
        }

        public Hex (CubicPos fromCube) : this()
        {
            this.Cube = fromCube;
        }

        public Hex (AxialPos fromAxial) : this()
        {
            this.Axial = fromAxial;
        }

        public Hex (OffsetPos fromOffset) : this()
        {
            this.Offset = fromOffset;
        }

        public bool Equals (Hex obj)
        {
            return this == (Hex)obj;
        }

		public override bool Equals (object obj)
		{
			return obj is Hex && this == (Hex)obj;
		}

        public override int GetHashCode ()
        {
            return canonical.GetHashCode();
        }

        public static bool operator == (Hex lhs, Hex rhs)
        {
            return lhs.canonical == rhs.canonical;
        }

        public static bool operator != (Hex lhs, Hex rhs)
        {
            return !(lhs.canonical == rhs.canonical);
        }

        public static Hex operator + (Hex lhs, Hex rhs)
        {
            return new Hex(lhs.Cube + rhs.Cube);
        }

        public static Hex operator - (Hex lhs, Hex rhs)
        {
            return new Hex(lhs.Cube - rhs.Cube);
        }

        /// <summary>
        /// Returns the clockwise next corner point of specified side in
        /// 2D projected coordinates.
        /// I.e., if Side is Side.TopRight, the returned corner will be on
        /// the right-hand side of the flat-top hexagon.
        /// </summary>
        /// <returns>The corner position, in grid scaled with radius</returns>
        /// <param name="side">The side for which to get corner point</param>
        /// <param name="radius">Radius of hex cells in grid</param>
        public Vector2 GetCorner (Side side, float radius)
        {
            float yDelta = (radius * Sqrt3) * 0.5f;
            Vector2 center = GetCenter(radius);

            switch (side)
            {
                case Side.TopLeft:
                    return new Vector2(
                        center.x - radius * 0.5f,
                        center.y - yDelta);
                case Side.Top:
                    return new Vector2(
                        center.x + radius * 0.5f,
                        center.y - yDelta);
                case Side.TopRight:
                    return new Vector2(center.x + radius, center.y);
                case Side.BottomRight:
                    return new Vector2(
                        center.x + radius * 0.5f,
                        center.y + yDelta);
                case Side.Bottom:
                    return new Vector2(
                        center.x - radius * 0.5f,
                        center.y + yDelta);
                case Side.BottomLeft:
                    return new Vector2(center.x - radius, center.y);
            }
            return center;
        }

        /// <summary>
        /// Returns the centerpoint of cell in 2D projected coordinates, scaled
        /// with the radius value.
        /// </summary>
        /// <returns>The center point scaled with radius</returns>
        /// <param name="radius">Radius of cells to scale the point with</param>
        public Vector2 GetCenter (float radius)
        {
			float yDelta = (radius * Sqrt3) * 0.5f;
            return new Vector2(
                Axial.X * radius * 1.5f,
                (2 * Axial.Y + Axial.X) * yDelta);
        }

        public void SetFromScreenCoordinate (int x, int y, int size)
        {
            float q = x * (2 / 3.0f) / (float)size;
            float r = (-x / 3 + Sqrt3 / 3.0f * y) / (float)size;

            float lx = q;
            float ly = -q - r;
            float lz = r;

            int rx = Mathf.RoundToInt(lx);
            int ry = Mathf.RoundToInt(ly);
            int rz = Mathf.RoundToInt(lz);

            float x_diff = Mathf.Abs(rx - lx);
            float y_diff = Mathf.Abs(ry - ly);
            float z_diff = Mathf.Abs(rz - lz);

            if (x_diff > y_diff && x_diff > z_diff)
                rx = -ry - rz;
            else if (y_diff > z_diff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            this.Cube = new CubicPos(rx, ry, rz);
        }

		public override string ToString ()
		{
			return string.Format ("[Hex: Cube={0}, Axial={1}, Offset={2}]", Cube, Axial, Offset);
		}
    }
}
