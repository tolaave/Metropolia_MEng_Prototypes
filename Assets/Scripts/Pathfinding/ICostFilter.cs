using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cost filter for the pathfinder. The generic
/// parameter CellType must be set to the type
/// implementing nodes in the map, and GraphPosition
/// must be the type used to store map coordinates.
/// </summary>
public interface ICostFilter<CellType,GraphPosition>
	where CellType : IPathFinderNodeSource<CellType,GraphPosition>
{
	/// <summary>
	/// Get pathfinder cost for the given map node.
	/// </summary>
	float GetCost(CellType source);
}
