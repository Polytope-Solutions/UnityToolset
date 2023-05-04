using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
		public static int NextWeightedIndex(this System.Random randomizer, float[] weights) {
			float totalWeight = 0f;
			weights.ToList().ForEach(weight => { totalWeight += weight; });
			float randomValue = ((float)randomizer.NextDouble()) * totalWeight;
			float currentTotal = 0f;
			int i = 0;
			for (i = 0; i < weights.Length-1; i++){
				currentTotal += weights[i];
				if (currentTotal >= randomValue)
					break;
			}
			return i;
		}
	
	}
}