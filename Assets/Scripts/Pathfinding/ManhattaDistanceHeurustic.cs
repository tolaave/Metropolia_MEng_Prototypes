using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Hex;

/// <summary>
/// A simle Manhattan distance heurustic on the hexagonal map grid.
/// </summary>
public class ManhattaDistanceHeurustic<CellType> :
	IHeuristic<CellType,Hex>
	where CellType : IPathFinderNodeSource<CellType,Hex>
{
	public float GetHeuristic(CellType source, CellType target)
	{
		// use the cubic hex distance function as heuristic
		return target.GetPosition().Cube.DistanceTo(
			source.GetPosition().Cube);
	}
}
