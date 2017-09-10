using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Hex;
using System.Linq;

/// <summary>
/// A generic reusable A* pathfinding engine. Uses four interfaces:
/// - IPathFinderSource to provide data about the source graph
/// - IPathFinderNodeSource to provide data for individual nodes
/// - ICostFilter to calculate node traversal costs
/// - IHeuristic for the A* H-cost estimation
/// 
/// Usage:
/// 
/// var pathFinder = new PathFinder<MyNodeType>(
///     graphSource, herustic, costFilter);
/// 
/// pathFinder.Update();
/// 
/// The HasPath property will indicate if any path was found,
/// and the path data is accessible through the Path property.
/// </summary>
public class PathFinder<CellType,GraphPosition>
	where CellType : IPathFinderNodeSource<CellType,GraphPosition>
	where GraphPosition : System.IEquatable<GraphPosition>
{
	/// <summary>
	/// Result node container for the path query. Used to
	/// track open and closed nodes, and provide the result path.
	/// </summary>
	public class PathFinderNode<CellType2,GraphPosition2>
		where CellType2 : IPathFinderNodeSource<CellType2,GraphPosition2>
		where GraphPosition2 : System.IEquatable<GraphPosition2>
	{
		/// <summary>
		/// Pathfinder which owns this node
		/// </summary>
		PathFinder<CellType2,GraphPosition2> finder;

		/// <summary>
		/// Parent node, from which the search was expanded to this
		/// node. Used to track the path back to start node.
		/// </summary>
		public PathFinderNode<CellType2,GraphPosition2> Parent { get; private set; }

		/// <summary>
		/// Original graph node which this internal node maps to
		/// </summary>
		public CellType2 Source { get; private set; }

		/// <summary>
		/// Actual cost of query up to this point. Sum of previous
		/// nodes' and this node's costs.
		/// </summary>
		public float G { get; private set; }

		/// <summary>
		/// Estimated remaining cost of this node; estimated distance to goal node.
		/// </summary>
		public float H { get; private set; }

		/// <summary>
		/// Heuristic function F = G + H result, estimated total path cost when using.
		/// this node.
		/// </summary>
		public float F
		{
			get
			{
				return G + H;
			}
		}

		/// <summary>
		/// Initialize the node container
		/// </summary>
		/// <param name="pf">Reference to the pathfinder which owns the node</param>
		/// <param name="parent">The parent node, which the query was expanded from</param>
		/// <param name="source">The actual graph node from user code, implementing IPathFinderNodeSource interface</param>
		/// <param name="cost">Total cost up to and including this node</param>
		public PathFinderNode(
			PathFinder<CellType2,GraphPosition2> pf,
			PathFinderNode<CellType2,GraphPosition2> parent,
			CellType2 source,
			float cost)
		{
			this.finder = pf;
			this.Source = source;
			this.Parent = parent;
			this.G = cost;
			this.H = finder.HeuristicFunction.GetHeuristic(Source, finder.Source.GetGoal());
		}
	}

	/// <summary>
	/// The query graph source data provider.
	/// </summary>
	public IPathFinderSource<CellType,GraphPosition> Source { get; private set; }

	/// <summary>
	/// Resulting path of the query
	/// </summary>
	public List<PathFinderNode<CellType,GraphPosition>> Path { get; private set; }

	private Dictionary<GraphPosition,PathFinderNode<CellType,GraphPosition>> closedNodes =
		new Dictionary<GraphPosition, PathFinderNode<CellType,GraphPosition>>();
	
	private PriorityQueue<PathFinderNode<CellType,GraphPosition>> openNodes =
		new PriorityQueue<PathFinderNode<CellType,GraphPosition>>();
	
	private bool finished;
	private PathFinderNode<CellType,GraphPosition> goalNode;

	/// <summary>
	/// Reference to the heuristic function currently active
	/// </summary>
	public IHeuristic<CellType,GraphPosition> HeuristicFunction { get; set; }

	/// <summary>
	/// The current pathfinding cost filter
	/// </summary>
	public ICostFilter<CellType,GraphPosition> CostFilter { get; set; }

	public bool HasPath
	{
		get
		{
			return Path != null;
		}
	}

	public PathFinder(
		IPathFinderSource<CellType,GraphPosition> source,
		IHeuristic<CellType,GraphPosition> heuristic,
		ICostFilter<CellType,GraphPosition> costFilter)
	{
		this.Source = source;
		this.HeuristicFunction = heuristic;
		this.CostFilter = costFilter;
	}

	/// <summary>
	/// Run the pathfinder query.
	/// </summary>
	public void Update()
	{
		float startTime = Time.realtimeSinceStartup;

		// Clear existing old data
		closedNodes.Clear();
		openNodes.Clear();
		finished = false;
		Path = null;

		// Add starting node to open list
		AddNode(Source.GetStart(), null);

		// Run until either goal found or no more open nodes are available
		while (!openNodes.Empty && !finished)
		{
			// Get highest-priority node from open list
			var node = openNodes.Dequeue();

			// Skip nodes that have already been processed
			if (closedNodes.ContainsKey(node.Source.GetPosition()))
				continue;

			// Add all neighbor nodes that can be traversed into in the open list
			var children = node.Source.GetNeighbors();
			foreach (var child in children)
			{
				// Skip empty neighbors, happens usually on map edges.
				if (child == null)
					continue;

				AddNode(child, node);
			}

			// Add this node to closed list
			closedNodes.Add(node.Source.GetPosition(), node);
		}

		// If finished is true, path was found
		if (finished)
		{
			// Trace backwards from goal node to start to build
			// list of result path nodes
			Path = new List<PathFinderNode<CellType,GraphPosition>>();
			var node = goalNode;
			while (node.Parent != null)
			{
				Path.Add(node);
				node = node.Parent;
			}
			Debug.Log("Found path, length: " + Path.Count + " cost: " + Path.First().G);
		}
		else
		{
			Debug.Log("No path");
		}

		float endTime = Time.realtimeSinceStartup;

		Debug.Log("Time spent: " + (endTime - startTime).ToString("0.000"));
	}

	private void AddNode(CellType source, PathFinderNode<CellType,GraphPosition> parent)
	{
		if (closedNodes.ContainsKey(source.GetPosition()))
			return;
		
		var parentCost = (parent != null) ? parent.G : 0.0f;

		// Ignore cost for start node
		var thisCost = (parent != null) ? CostFilter.GetCost(source) : 0.0f;

		// Create and insert new node into open list
		var newNode = new PathFinderNode<CellType,GraphPosition>(
			              this,
			              parent,
			              source,
			              thisCost + parentCost);
		openNodes.Insert(newNode.F, newNode);

		// Check if goal reached
		if (source.GetPosition().Equals(Source.GetGoal().GetPosition()))
		{
			finished = true;
			goalNode = newNode;
		}

		openNodes.Validate();
	}
}
