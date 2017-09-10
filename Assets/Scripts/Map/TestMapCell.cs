using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Hex;

/// <summary>
/// Cell for the prototype map.
/// </summary>
public class TestMapCell :
	MonoBehaviour,
	IPathFinderNodeSource<TestMapCell,Hex>
{
	/// <summary>
	/// The type of this map cell
	/// </summary>
	public enum NodeType
	{
		Plain,
		Obstacle,
		Start,
		Goal
	}

	private TestMap map;

	/// <summary>
	/// Position of this cell on the map
	/// </summary>
	private Hex pos;

	// MonoBehaviour Fields exposed to Unity Editor
	public SpriteRenderer cellSprite;
	public SpriteRenderer spotSprite;
	public SpriteRenderer borderSprite;

	public NodeType Type { get; private set; }

	private bool _spot = false;
	/// <summary>
	/// Toggle the spot visualization, used to visualize
	/// the pathfinder result on the map.
	/// </summary>
	public bool Spot
	{
		get { return _spot; }
		set { _spot = value; UpdateSpot(); }
	}

	private bool _visited = false;
	/// <summary>
	/// The visited flag, used to visualize nodes
	/// that were visited during pathfinding.
	/// </summary>
	public bool Visited
	{
		get { return _visited; }
		set { _visited = value; UpdateBorders(); }
	}

	public void Init(TestMap map, Hex pos, NodeType type)
	{
		this.map = map;
		this.pos = pos;

		// Set physical world position of this cell
		// based on the hex location
		Vector2 v2 = pos.GetCenter(0.5f);
		transform.position = new Vector3(v2.x, v2.y, 0.0f);

		SetType(type);
		UpdateSpot();
	}

	/// <summary>
	/// Handle user input when this map cell is clicked.
	/// </summary>
	public void OnMouseDown()
	{
		map.ClickMap(pos);
	}

	/// <summary>
	/// Update the node type. Also updates spatial database
	/// visualization.
	/// </summary>
	/// <param name="type">New type of the node</param>
	public void SetType(NodeType type)
	{
		Type = type;
		switch (type)
		{
			case NodeType.Goal:
				// Goal node, always black
				{
					cellSprite.color = Color.black;
					break;
				}
			case NodeType.Obstacle:
				// Obstacle node, always white
				{
					cellSprite.color = Color.white;
					break;
				}
			case NodeType.Plain:
				// Plain node
				{
					var value = map.SpatialDatabase.GetData(pos.Offset.X, pos.Offset.Y);
					switch (map.SpatialDatabase.ActiveLayer)
					{
						case TestMap.SpatialDataLayer.MovementCost:
							// Movement colors range from green (lowest) to red (highest)
							{
								value /= 8.0f;
								cellSprite.color = new Color(value, 1.0f - value, 0.0f);
								break;
							}
						case TestMap.SpatialDataLayer.OwnInfluence:
							// Own influence colors range from gray (lowest) to green (highest)
							{
								value /= 4.0f;
								cellSprite.color = new Color(0.5f, 0.5f + value / 2.0f, 0.5f);
								break;
							}
						case TestMap.SpatialDataLayer.EnemyInfluence:
							// Enemy influence colors range from gray (lowest) to red (highest)
							{
								value /= 4.0f;
								cellSprite.color = new Color(0.5f + value / 2.0f, 0.5f, 0.5f);
								break;
							}
						case TestMap.SpatialDataLayer.CombinedInfluence:
							// Combined influence colors range from red (enemy) to green (own)
							{
								value /= 4.0f;
								cellSprite.color = new Color(
									0.5f - value * 0.5f,
									0.5f + value * 0.5f,
									0.0f);
								break;
							}
						case TestMap.SpatialDataLayer.Tension:
							// Tension colors range from green (low) to red (high)
							{
								value /= 4.0f;
								cellSprite.color = new Color(value * 0.5f, 1.0f - value * 0.5f, 0.0f);
								break;
							}
						case TestMap.SpatialDataLayer.Vulnerability:
							// Vulnerability colors range from green (low) to red (high)
							{
								value /= 4.0f;
								cellSprite.color = new Color(value * 0.5f, 1.0f - value * 0.5f, 0.0f);
								break;
							}
					}
					break;
				}
			case NodeType.Start:
				// Start node, always yellow
				{
					cellSprite.color = Color.yellow;
					break;
				}
		}
	}

	/// <summary>
	/// Refresh the visualization of this node
	/// </summary>
	public void Refresh()
	{
		SetType(Type);
	}

	private void UpdateSpot()
	{
		spotSprite.gameObject.SetActive(_spot);
	}
		
	private void UpdateBorders()
	{
		borderSprite.color = _visited ? Color.black : Color.white;
	}

	/// <summary>
	/// Returns this node's position to the pathfinder
	/// </summary>
	public Hex GetPosition()
	{
		return pos;		
	}

	/// <summary>
	/// Get list of neighbor nodes of this for the pathfinder
	/// </summary>
	public TestMapCell[] GetNeighbors()
	{
		Visited = true;

		int i = 0;
		var result = new TestMapCell[(int)Hex.Side.NumSides];
		foreach (var side in Hex.directions.Keys)
		{
			var dirPos = Hex.directions[side];
			var newPos = pos + new Hex(dirPos);
			var node = map.GetNodeAt(newPos);
			if (node != null && node.Type != NodeType.Obstacle)
			{
				result[i++] = node;
			}
		}
		return result;
	}
}
