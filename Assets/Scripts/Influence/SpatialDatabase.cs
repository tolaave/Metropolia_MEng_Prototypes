using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple spatial database implementation for two-dimensional map.
/// The generic types that must be defined are:
/// - TMap: type of the source map implementing IInfluenceMap inteface
/// - TInfluenceSource: type of influence sources, i.e. units on the map
/// - TLayerEnum: the Enum type which defines list of layers in the database
/// </summary>
public class SpatialDatabase<TMap,TInfluenceSource,TLayerEnum>
	where TMap : IInfluenceMap<TInfluenceSource,TLayerEnum>
	where TInfluenceSource : IInfluenceSource
	where TLayerEnum : struct, System.IConvertible
{
	public TMap Source { get; private set; }
	public int Width { get; private set; }
	public int Height { get; private set; }
	public TLayerEnum ActiveLayer { get; set; }

	/// <summary>
	/// Contains the actual data in spatial database. Each type
	/// in the TLayerEnum has a matching two-dimensional influence
	/// map with floating-point values.
	/// </summary>
	private Dictionary<TLayerEnum, float[,]> layers = new Dictionary<TLayerEnum, float[,]>();

	public SpatialDatabase(TMap source, int width, int height)
	{
		this.Source = source;
		this.Width = width;
		this.Height = height;

		// Get list of values in the TLayerEnum type and create the
		// influence layers
		var layerEnumValues = System.Enum.GetValues(typeof(TLayerEnum));
		foreach (var layer in layerEnumValues)
		{
			layers[(TLayerEnum)layer] = new float[Width, Height];
		}
	}

	/// <summary>
	/// Sets influence value in the active layer
	/// </summary>
	public void SetData(int x, int y, float value)
	{
		SetData(x, y, value, ActiveLayer);
	}

	/// <summary>
	/// Sets influence value in specified layer
	/// </summary>
	public void SetData(int x, int y, float value, TLayerEnum layer)
	{
		var layerData = layers[layer];
		layerData[x, y] = value;
	}

	/// <summary>
	/// Adds influence to the active layer
	/// </summary>
	public void AddData(int x, int y, float value)
	{
		AddData(x, y, value, ActiveLayer);
	}

	/// <summary>
	/// Adds influence to specified layer
	/// </summary>
	public void AddData(int x, int y, float value, TLayerEnum layer)
	{
		var layerData = layers[layer];
		layerData[x, y] += value;
	}

	/// <summary>
	/// Gets influence level from the active layer
	/// </summary>
	public float GetData(int x, int y)
	{
		var layerData = layers[ActiveLayer];
		return layerData[x, y];
	}

	/// <summary>
	/// Gets influence level from specified layer
	/// </summary>
	public float GetData(int x, int y, TLayerEnum layer)
	{
		var layerData = layers[layer];
		return layerData[x, y];
	}

	/// <summary>
	/// Recalculates the data in all influence layers.
	/// </summary>
	public void RecalculateInfluence()
	{
		// Go through all layers in order
		foreach (var layer in layers.Keys)
		{
			var layerData = layers[layer];

			// Get influence sources for this layer
			var sources = Source.GetInfluenceSources(layer);
			if (sources != null)
			{
				// Apply each influence source
				foreach (var source in sources)
				{
					Source.PropagateInfluence(layer, source);
				}
			}

			// Apply influence filter on each node on this layer.
			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
				{
					AddData(x, y, Source.FilterLayerInfluence(layer, x, y), layer);
				}
		}
	}
}
