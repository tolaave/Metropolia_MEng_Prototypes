using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Hex;

/// <summary>
/// Unit for the prototype map.
/// </summary>
public class TestUnit : MonoBehaviour, IInfluenceSource
{
	/// <summary>
	/// Reference to map owning this unit.
	/// </summary>
	public TestMap map;

	/// <summary>
	/// Position of this cell on the map.
	/// </summary>
	public Hex Pos { get; private set; }

	/// <summary>
	/// Unique ID of this unit.
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Flag to indicate whether this is friendly or enemy unit.
	/// </summary>
	/// <value><c>true</c> if friendly; otherwise, <c>false</c>.</value>
	public bool Friendly { get; private set; }
	public int Strength { get; private set; }

	public SpriteRenderer sprite;
	public TextMesh label; 

	public void Init(TestMap map, Hex pos, int id, int strength, bool friendly)
	{
		this.map = map;
		this.Pos = pos;
		this.Id = id;
		this.Strength = strength;
		this.Friendly = friendly;

		// Set physical world position of this cell
		// based on the hex location
		Vector2 v2 = pos.GetCenter(0.5f);
		transform.position = new Vector3(v2.x, v2.y, 0.0f);

		// Update unit sprite color, and set strength value as label
		sprite.color = Friendly ? Color.green : Color.red;
		label.text = "" + Strength;
	}
}
