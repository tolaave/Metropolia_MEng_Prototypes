using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Hex;

/// <summary>
/// Main interface of pathfinder for the map.
/// The generic parameter CellType must be set to the
/// type implementing nodes in the map, and
/// GraphPosition must be the type used to store map
/// coordinates.
/// </summary>
public interface IPathFinderSource<CellType,GraphPosition>
	where CellType : IPathFinderNodeSource<CellType,GraphPosition>
{
	/// <summary>
	/// Gets the start node
	/// </summary>
	CellType GetStart ();

	/// <summary>
	/// Gets the goal node
	/// </summary>
	CellType GetGoal ();

	/// <summary>
	/// Gets the node at a given location on the graph.
	/// </summary>
	CellType GetNodeAt (GraphPosition pos);
}
