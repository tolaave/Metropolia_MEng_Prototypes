using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for the pathfinder heuristic function.
/// The generic parameter CellType must be set to the
/// type implementing nodes in the map, and
/// GraphPosition must be the type used to store map
/// coordinates.
/// </summary>
public interface IHeuristic<CellType,GraphPosition>
	where CellType : IPathFinderNodeSource<CellType,GraphPosition>
{
	/// <summary>
	/// Calculate the heuristic value for given
	/// source and target nodes.
	/// </summary>
	/// <returns>The estimated heuristic value.</returns>
	/// <param name="source">Source node.</param>
	/// <param name="target">Target node.</param>
	float GetHeuristic(CellType source, CellType target);
}
