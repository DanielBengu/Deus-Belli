using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UERandom =  UnityEngine;

public static class RandomManager
{
	public static int GetRandomValue(int seed, int minInclusive, int maxExclusive)
	{
		UERandom.Random.InitState(seed);
		return UERandom.Random.Range(minInclusive, maxExclusive);
	}

	//Second part of dictionary represents the weight of the type of tile
	public static T GetRandomValueWithWeights<T>(int seed, Dictionary<T, int> elements)
	{
		UERandom.Random.InitState(seed);

		var weights = elements.SelectMany(pair => Enumerable.Repeat(pair.Key, pair.Value)).ToArray();
		int weightIndex = GetRandomValue(seed, 0, weights.Length);

		return weights[weightIndex];
	}
}