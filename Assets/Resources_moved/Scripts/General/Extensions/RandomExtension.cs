using System.Collections.Generic;
using UERandom =  UnityEngine;

public static class RandomManager
{
	public static int GetRandomValue(int seed, int minInclusive, int maxExclusive)
	{
		UERandom.Random.InitState(seed);
		return UERandom.Random.Range(minInclusive, maxExclusive);
	}
}