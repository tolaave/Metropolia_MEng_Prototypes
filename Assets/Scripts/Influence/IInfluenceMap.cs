using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for the source map of influence.
/// - TSource: type of the IInfluenceSource implementation
/// - TLayerEnum: the Enum type which defines list of
///   layers in the database
/// </summary>
public interface IInfluenceMap<TSource,TLayerEnum>
	where TSource : IInfluenceSource
{
	/// <summary>
	/// Gets the influence sources for given layer. May
	/// return null or empty array.
	/// </summary>
	IEnumerable<TSource> GetInfluenceSources(TLayerEnum layer);

	/// <summary>
	/// Propagates the influence on specified layer from
	/// the given influence source.
	/// </summary>
	void PropagateInfluence(TLayerEnum layer, TSource source);

	/// <summary>
	/// Filters the layer influence of specified layer at
	/// the given X,Y-coordinate.
	/// </summary>
	/// <returns>The resulting layer influence.</returns>
	float FilterLayerInfluence(TLayerEnum layer, int x, int y);
}
