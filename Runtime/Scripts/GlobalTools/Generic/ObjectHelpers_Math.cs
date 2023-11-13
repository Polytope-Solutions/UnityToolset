using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
		public static int NextWeightedIndex(this System.Random randomizer, List<float> weights) {
			float totalWeight = 0f;
			weights.ForEach(weight => { totalWeight += weight; });
			float randomValue = ((float)randomizer.NextDouble()) * totalWeight;
			float currentTotal = 0f;
			int i = 0;
			for (i = 0; i < weights.Count-1; i++){
				currentTotal += weights[i];
				if (currentTotal >= randomValue)
					break;
			}
			return i;
		}

		public static float FlipReference(this float value) {
            return value + 1 - 2*value.Fraction();
        }
		public static float Fraction(this float value) {
            return (float)((decimal)value % 1);
        }
	}
}