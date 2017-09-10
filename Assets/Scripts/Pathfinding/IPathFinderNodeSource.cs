using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Hex;

/// <summary>
/// Interface of pathfinder graph nodes for the pathfinder.
/// This should be implemented by the map cell nodes. The generic
/// parameter CellType must be set to the type implementing
/// this interface, and GraphPosition must be the type used
/// to store map coordinates.
/// </summary>
public interface IPathFinderNodeSource<CellType,GraphPosition>
	where CellType : IPathFinderNodeSource<CellType,GraphPosition>
{
	/// <summary>
	/// Get the position of node on graph. Used
	/// also as key for tracking visited nodes.
	/// </summary>
	GraphPosition GetPosition();

	/// <summary>
	/// Get list of neighbor nodes of this node
	/// in the graph.
	/// </summary>
	CellType[] GetNeighbors();
}
