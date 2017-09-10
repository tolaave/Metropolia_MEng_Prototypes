using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Hex;
using System.Linq;

/// <summary>
/// Map prototype. Used to test pathfinding and influence maps.
/// </summary>
public class TestMap :
	MonoBehaviour,
	IPathFinderSource<TestMapCell,Hex>,
	IInfluenceMap<TestUnit,TestMap.SpatialDataLayer>
{
	/// <summary>
	/// Layers used for spatial database
	/// </summary>
	public enum SpatialDataLayer
	{
		MovementCost,
		OwnInfluence,
		EnemyInfluence,
		CombinedInfluence,
		Tension,
		Vulnerability
	}

	/// <summary>
	/// Different modes for the pathfinder
	/// </summary>
	public enum PathfinderMode
	{
		Simple,
		MovementCost,
		AvoidEnemy
	}

	/// <summary>
	/// Map width
	/// </summary>
	public const int Width = 20;

	/// <summary>
	/// Map height
	/// </summary>
	public const int Height = 20;

	// MonoBehaviour fields exposed to Unity editor

	public GameObject mapCellPrefab;
	public GameObject mapUnitPrefab;
	public Transform mapRoot;
	public Transform unitsRoot;
	public Camera cameraRef;
	public int numUnits = 6;
	public PathfinderMode pathfinderMode;

	public TestUnit ActiveUnit { get; private set; }
	public SpatialDatabase<TestMap,TestUnit,SpatialDataLayer>
		SpatialDatabase { get; private set; }
	public PathFinder<TestMapCell,Hex> PathFinder { get; private set; }

	private TestMapCell[,] map;
	private Dictionary<int,TestUnit> units = new Dictionary<int, TestUnit>();
	private Hex? currentTarget;
	private Hex? currentStart;

	/// <summary>
	/// Test cost filter, used by pathfinder to query graph
	/// traversal costs for given nodes.
	/// </summary>
	private class TestCostFilter : ICostFilter<TestMapCell,Hex>
	{
		public TestMap map;

		/// <summary>
		/// Just passes the query to TestMap.GetPathfinderCost method
		/// </summary>
		public float GetCost(TestMapCell source)
		{
			return map.GetPathfinderCost(source.GetPosition());
		}
	}

	public void Start()
	{
		// Create the spatial database for this map
		SpatialDatabase = new SpatialDatabase<TestMap,TestUnit,SpatialDataLayer>(
			this, Width, Height);

		// Create map
		map = new TestMapCell[Width, Height];
		for (int y = 0; y < Height; y++)
			for (int x = 0; x < Width; x++)
			{
				// Convert offset coordinate to hex position
				var hexPos = new Hex (new OffsetPos (x, y));

				// Instantiate map cell prefab on the map
				var cellGO = Instantiate(mapCellPrefab);
				cellGO.transform.SetParent(mapRoot);
				var cell = cellGO.GetComponent<TestMapCell>();

				// Initialize the map cell with 1/6 (≈16%) chance of being obstacle
				cell.Init(this, hexPos, 
					(Random.Range(1,6) == 1) ? TestMapCell.NodeType.Obstacle :
					TestMapCell.NodeType.Plain);

				// Update spatial database movement cost layer with random
				// cost between 1..8
				SpatialDatabase.SetData(x, y, (float)Random.Range(1, 8), SpatialDataLayer.MovementCost);

				map[x, y] = cell;
			}

		// Create units
		int id = 0;
		for (int i = 0; i < numUnits; i++)
		{
			// p = 0 for friendly, p = 1 for enemy
			for (int p = 0; p < 2; p++)
			{
				// Pick random position which is not obstacle, and does not contain any unit yet
				var pos = GetRandomPos((n) =>
					n.Type == TestMapCell.NodeType.Plain &&
					GetUnitAt(n.GetPosition()) == null);

				// Instantiate unit prefab on the map
				var unitGO = Instantiate(mapUnitPrefab);
				unitGO.transform.SetParent(unitsRoot);
				var unit = unitGO.GetComponent<TestUnit>();

				// Initialize the unit with random strength in range 2..6
				// and set friendly flag
				unit.Init(this, pos, id, Random.Range(2,6), p == 0);

				units[id++] = unit;
			}
		}

		// Update spatial database
		SpatialDatabase.RecalculateInfluence();

		// Create pathfinder instance
		PathFinder = new PathFinder<TestMapCell,Hex>(
			this,
			new ManhattaDistanceHeurustic<TestMapCell>(),
			new TestCostFilter { map = this });
		
		FixCamera();

		// Set random unit as active unit, and pick random goal
		SetActiveUnit(GetRandomUnit((u) => u.Friendly));
		SetTarget(GetRandomPos((n) => n.Type == TestMapCell.NodeType.Plain));
	}

	/// <summary>
	/// Get random position which matches the given filter.
	/// </summary>
	/// <returns>The random position.</returns>
	/// <param name="filter">Filter to match map hexes with.</param>
	private Hex GetRandomPos(System.Func<TestMapCell,bool> filter)
	{
		Hex pos;
		// Loop until a location is picked which passes the filter
		do
		{
			pos = new Hex(new OffsetPos(
				Random.Range(0, Width - 1),
				Random.Range(0, Height - 1)));
		}
		while (!filter(GetNodeAt(pos)));
		return pos;
	}

	/// <summary>
	/// Get random unit which matches the given filter.
	/// </summary>
	/// <returns>The randomly picked unit, null if no
	/// units exist for the given filter.</returns>
	/// <param name="filter">Filter to match units with.</param>
	private TestUnit GetRandomUnit(System.Func<TestUnit,bool> filter)
	{
		// Use LINQ OrderBy with Random to randomize order
		var rnd = new System.Random();
		return units.Values.Where(u => filter(u)).OrderBy(u => rnd.Next()).FirstOrDefault();
	}

	/// <summary>
	/// Set the active unit, updates pathfinder start
	/// position and clears old route
	/// </summary>
	/// <param name="unit">Unit to select.</param>
	private void SetActiveUnit(TestUnit unit)
	{
		ActiveUnit = unit;
		SetStart(unit.Pos);
		ClearRoute();
	}

	/// <summary>
	/// Gets the unit at given location
	/// </summary>
	/// <returns>Reference to the unit at this location,
	/// null if no unit here.</returns>
	/// <param name="pos">Position on map.</param>
	public TestUnit GetUnitAt(Hex pos)
	{
		return units.Values.FirstOrDefault(u => u.Pos == pos);
	}

	/// <summary>
	/// Gets the map node at given location
	/// </summary>
	/// <returns>Reference to the map cell at this location,
	/// null if out of range.</returns>
	/// <param name="pos">Position on map.</param>
	public TestMapCell GetNodeAt (Hex pos)
	{
		var offs = pos.Offset;
		if (!offs.InRange(0, 0, Width - 1, Height - 1))
			return null;
		return map[offs.X, offs.Y];
	}

	/// <summary>
	/// Used to provide the start node to pathfinder
	/// </summary>
	/// <returns>The start node.</returns>
	public TestMapCell GetStart ()
	{
		return this.GetNodeAt(currentStart.Value);
	}

	/// <summary>
	/// Used to provide the goal node to pathfinder
	/// </summary>
	/// <returns>The goal node.</returns>
	public TestMapCell GetGoal ()
	{
		return this.GetNodeAt(currentTarget.Value);
	}

	/// <summary>
	/// Gets the pathfinder cost for given hex cell.
	/// Uses the selected pathfinder mode to choose
	/// the appropriate source(s) and returns the
	/// resulting cost.
	/// </summary>
	/// <returns>The cost of given hex.</returns>
	/// <param name="pos">Position on map to query.</param>
	public float GetPathfinderCost(Hex pos)
	{
		int x = pos.Offset.X, y = pos.Offset.Y;
		switch (pathfinderMode)
		{
			case PathfinderMode.Simple:
				// For simple cost always return 1 for each node
				return 1;
			case PathfinderMode.MovementCost:
				// Return the terrain movement cost value
				return SpatialDatabase.GetData(x, y, SpatialDataLayer.MovementCost);
			case PathfinderMode.AvoidEnemy:
				// When avoiding enemy, use 10x score from enemy influence,
				// but add 1x movement cost to allow units to optimize
				// terrain movement when no enemies nearby
				return 10 * SpatialDatabase.GetData(x, y, SpatialDataLayer.EnemyInfluence) +
					SpatialDatabase.GetData(x, y, SpatialDataLayer.MovementCost);
		}
		return 1;
	}

	/// <summary>
	/// Set pathfinder start position
	/// </summary>
	/// <param name="start">Start location</param>
	public void SetStart(Hex start)
	{
		if (currentStart.HasValue)
		{
			var oldOffs = currentStart.Value.Offset;
			map[oldOffs.X, oldOffs.Y].SetType(TestMapCell.NodeType.Plain);
		}
		var offs = start.Offset;
		map[offs.X, offs.Y].SetType(TestMapCell.NodeType.Start);
		currentStart = start;
	}

	/// <summary>
	/// Handles user input (mouse click) on the map.
	/// When clicking friendly unit, make it the new
	/// pathfinding source, otherwise set the clicked
	/// hex as pathfinder goal.
	/// </summary>
	/// <param name="pos">Position clicked.</param>
	public void ClickMap(Hex pos)
	{
		var unit = GetUnitAt(pos);
		if (unit != null && unit.Friendly)
		{
			SetActiveUnit(unit);
		}
		else
		{
			SetTarget(pos);
		}
	}

	/// <summary>
	/// Clear the old pathfinder route visualization
	/// and target.
	/// </summary>
	private void ClearRoute()
	{
		for (int y = 0; y < Height; y++)
			for (int x = 0; x < Width; x++)
			{
				map[x, y].Spot = false;
				map[x, y].SetType(map[x, y].Type);
				map[x, y].Visited = false;
			}

		if (currentTarget.HasValue)
		{
			var oldOffs = currentTarget.Value.Offset;
			map[oldOffs.X, oldOffs.Y].SetType(TestMapCell.NodeType.Plain);
			currentTarget = null;
		}
	}

	/// <summary>
	/// Force map refresh, updates the hex cell colors to
	/// match currently active spatial database layer.
	/// </summary>
	public void RefreshMapColors()
	{
		for (int y = 0; y < Height; y++)
			for (int x = 0; x < Width; x++)
			{
				map[x, y].Refresh();
			}
	}

	/// <summary>
	/// Set new pathfinder target. Clears the old path, and
	/// triggers new pathfinder query, updating the map
	/// spots if path was found.
	/// </summary>
	/// <param name="target">Target.</param>
	public void SetTarget(Hex target)
	{
		ClearRoute();

		var offs = target.Offset;
		var cell = map[offs.X, offs.Y];
		if (cell.Type == TestMapCell.NodeType.Plain)
		{
			cell.SetType(TestMapCell.NodeType.Goal);
			currentTarget = target;
		}

		if (currentTarget.HasValue)
		{
			PathFinder.Update();
			if (PathFinder.HasPath)
			{
				foreach (var pfNode in PathFinder.Path)
				{
					var node = pfNode.Source;
					node.Spot = true;
				}
			}
		}
	}

	/// <summary>
	/// Refresh the pathfinder path. Basically
	/// just sets the target to its current value
	/// to trigger pathfinder update.
	/// </summary>
	private void UpdatePath()
	{
		if (currentTarget.HasValue)
		{
			SetTarget(currentTarget.Value);
		}
	}

	/// <summary>
	/// Resize and position camera to fit map in the
	/// orthogonal viewport
	/// </summary>
	private void FixCamera()
	{
		var bottomLeftHex = new Hex(new OffsetPos(0, 0));
		var topRightHex = new Hex(new OffsetPos(Width, Height));

		var bottomLeft = bottomLeftHex.GetCenter(0.5f);
		var topRight = topRightHex.GetCenter(0.5f);

		var center = (bottomLeft + topRight) / 2.0f;
		center.y -= Hex.Sqrt3 * 0.25f * 0.5f;
		center.x -= Hex.Sqrt3 * 0.25f * 0.5f;

		cameraRef.transform.position = new Vector3(center.x, center.y, cameraRef.transform.position.z);
		cameraRef.orthographicSize = Mathf.Abs(topRight.x - bottomLeft.x + Hex.Sqrt3) * 0.5f / cameraRef.aspect;
	}

	/// <summary>
	/// Gets the influence sources. Used by Spatial Database.
	/// </summary>
	/// <returns>The influence sources which affect the given layer.</returns>
	/// <param name="layer">Layer which source affect</param>
	public IEnumerable<TestUnit> GetInfluenceSources(SpatialDataLayer layer)
	{
		if (layer == SpatialDataLayer.EnemyInfluence)
		{
			// Enemy influence is affected by enemy unit strengths
			return units.Values.Where(u => !u.Friendly);
		}
		else if (layer == SpatialDataLayer.OwnInfluence)
		{
			// Own influence is affected by friendly unit strengths
			return units.Values.Where(u => u.Friendly);
		}
		else
			return null;
	}

	/// <summary>
	/// Propagates the influence of given source to this layer
	/// </summary>
	/// <param name="layer">Layer to output influence on.</param>
	/// <param name="source">The influence source.</param>
	public void PropagateInfluence(SpatialDataLayer layer, TestUnit source)
	{
		int strength = source.Strength;

		// Add unit location influence first
		SpatialDatabase.AddData(source.Pos.Offset.X, source.Pos.Offset.Y, strength, layer);
		// Spread influence linearly, reducing effect by one for
		// each step further from the source unit. Each iteration
		// applies the influence as a hexagonal ring around the 
		// source.
		for (int i = 1; i <= strength; i++)
		{
			// Apply to each hex direction
			foreach (var dir in Hex.directions)
			{
				// Get one edge of this hexagonal circle to start
				// from. Loop for the number of hexes
				// on this particular side to apply to.
				var rimPos = source.Pos.Cube + dir.Value * i;
				for (int r = 0; r < i; r++)
				{
					// The ring direction is two steps clockwise from
					// the edge direction:
					//
					//   P-_        O: unit location
					//   |  -T      P: circle corner (rimPos)
					//   |          T: next corner (2 steps in clockwise
					//   O             direction) to iterate towards
					//
					// Each iteration of [r] steps from P towards T
					rimPos += Hex.directions[(Hex.Side)(((int)dir.Key + 2) % (int)Hex.Side.NumSides)];
					var rimHex = new Hex(rimPos);
					// Check range in case we hit edge of map
					if (rimHex.Offset.InRange(0, 0, Width - 1, Height - 1))
					{
						SpatialDatabase.AddData(rimHex.Offset.X, rimHex.Offset.Y, strength - i, layer);
					}
				}
			}
		}
	}

	/// <summary>
	/// Layer influence filter. Used to combine lower layers into
	/// higher-level layers.
	/// </summary>
	/// <returns>The output value of influence for this cell.</returns>
	/// <param name="layer">Layer being filtered.</param>
	/// <param name="x">The x coordinate on map.</param>
	/// <param name="y">The y coordinate on map.</param>
	public float FilterLayerInfluence(SpatialDataLayer layer, int x, int y)
	{
		switch (layer)
		{
			case SpatialDataLayer.CombinedInfluence:
				// combinedInfluence = ownInfluence - enemyInfluence
				{
					return SpatialDatabase.GetData(x, y, SpatialDataLayer.OwnInfluence) -
						SpatialDatabase.GetData(x, y, SpatialDataLayer.EnemyInfluence);
				}
			case SpatialDataLayer.Tension:
				// tensionLevel = ownInfluence + enemyInfluence
				{
					return SpatialDatabase.GetData(x, y, SpatialDataLayer.OwnInfluence) +
						SpatialDatabase.GetData(x, y, SpatialDataLayer.EnemyInfluence);
				}
			case SpatialDataLayer.Vulnerability:
				// vulnerabilityLevel = tensionLevel - Abs(combinedInfluence)
				{
					return SpatialDatabase.GetData(x, y, SpatialDataLayer.Tension) -
						Mathf.Abs(SpatialDatabase.GetData(
							x,
							y,
							SpatialDataLayer.CombinedInfluence));
				}
		}
		return 0.0f;
	}

	/// <summary>
	/// Show the debug buttons on top of the game view
	/// </summary>
	void OnGUI()
	{
		// Button grid for selecting active pathfinder mode
		var newPathfinderMode = (PathfinderMode)GUILayout.SelectionGrid(
            (int)pathfinderMode,
            System.Enum.GetNames(typeof(PathfinderMode)),
            System.Enum.GetValues(typeof(PathfinderMode)).Length);
		if (newPathfinderMode != pathfinderMode)
		{
			pathfinderMode = newPathfinderMode;
			UpdatePath();
		}

		// Button grid for selecting which spatial database layer
		// is being visualized on map
		var newActiveLayer = (SpatialDataLayer)GUILayout.SelectionGrid(
			(int)SpatialDatabase.ActiveLayer,
			System.Enum.GetNames(typeof(SpatialDataLayer)),
			System.Enum.GetValues(typeof(SpatialDataLayer)).Length);
		if (newActiveLayer != SpatialDatabase.ActiveLayer)
		{
			SpatialDatabase.ActiveLayer = newActiveLayer;
			RefreshMapColors();
		}
	}
}
