using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class SmoothNoise
{
	public static float Generate(float x, float scale)
	{
		x /= scale;

		float lower = GetRandomValue(Mathf.FloorToInt(x));
		float higher = GetRandomValue(Mathf.CeilToInt(x));

		return Mathf.Lerp(lower, higher, x);
	}


	static float GetRandomValue(int seed)
	{
		return (1103515245 * seed + 12345) % int.MaxValue;
	}
}

