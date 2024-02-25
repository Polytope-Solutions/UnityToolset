using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
        public abstract class MinMaxArray <T> {
            [SerializeField] protected T[] min;
            [SerializeField] protected T[] max;
            protected int dimensionSize;
            public bool IsMatchingDimension(int _dimension) => this.dimensionSize == _dimension;
            public bool IsValidIndex(int _dimension) => _dimension < this.dimensionSize;
            protected abstract T MinValue { get; }
            protected abstract T MaxValue { get; }
            protected abstract T DefaultInvalidValue { get; }
            protected abstract T Difference(T _value1, T _value2);
            protected abstract T Min(T _value1, T _value2);
            protected abstract T Max(T _value1, T _value2);
            protected abstract float NormalizedValue(T _min, T _max, T _value);
            public T Min(int _dimension) => this.IsValidIndex(_dimension) ? this.min[_dimension] : this.DefaultInvalidValue;
            public T Max(int _dimension) => this.IsValidIndex(_dimension) ? this.max[_dimension] : this.DefaultInvalidValue;
            public T[] Range {
                get {
                    T[] size = new T[this.dimensionSize];
                    for (int i = 0; i < this.dimensionSize; i++) {
                        size[i] = this.Difference(this.max[i], this.min[i]);
                    }
                    return size;
                }
            }
            public MinMaxArray(int _dimensionSize) {
                this.dimensionSize = _dimensionSize;
                this.min = new T[_dimensionSize];
                this.max = new T[_dimensionSize];
                for (int i = 0; i < _dimensionSize; i++) {
                    this.min[i] = this.MaxValue;
                    this.max[i] = this.MinValue;
                }
            }
            public void Update(T[] values) {
                if (!this.IsMatchingDimension(values.Length)) return;
                for (int i = 0; i < this.dimensionSize; i++) {
                    this.min[i] = this.Min(this.min[i], values[i]);
                    this.max[i] = this.Max(this.max[i], values[i]);
                }
            }

            public override string ToString() {
                return $"Min:[{string.Join(",", this.min)}],Max[{string.Join(",", this.max)}]";
            }
            public float[] Evaluate(T[] values) {
                if (!this.IsMatchingDimension(values.Length)) return null;
                float[] normalizedValues = new float[this.dimensionSize];
                for (int i = 0; i < this.dimensionSize; i++) {
                    normalizedValues[i] = Evaluate(i, values[i]);
                }
                return normalizedValues;
            }
            public float Evaluate(int dimension, T value) {
                float normalizedValue = -1f;
                if (!this.IsMatchingDimension(dimension)) return normalizedValue;
                normalizedValue = this.NormalizedValue(this.min[dimension], this.max[dimension], value);
                return normalizedValue;
            }
        }
        [System.Serializable]
        public class FloatMinMaxArray : MinMaxArray<float> {
            protected override float MinValue => float.MinValue;
            protected override float MaxValue => float.MaxValue;
            protected override float DefaultInvalidValue => -1f;
            protected override float Difference(float _value1, float _value2) => _value1 - _value2;
            protected override float Min(float _value1, float _value2) => Mathf.Min(_value1, _value2);
            protected override float Max(float _value1, float _value2) => Mathf.Max(_value1, _value2);
            protected override float NormalizedValue(float _min, float _max, float _value) => Mathf.InverseLerp(_min, _max, _value);


            public FloatMinMaxArray(int _dimensionSize) : base(_dimensionSize) { }
        }
        [System.Serializable]
        public class DoubleMinMaxArray : MinMaxArray<double> {
            protected override double MinValue => double.MinValue;
            protected override double MaxValue => double.MaxValue;
            protected override double DefaultInvalidValue => -1;
            protected override double Difference(double _value1, double _value2) => _value1 - _value2;
            protected override double Min(double _value1, double _value2) => Math.Min(_value1, _value2);
            protected override double Max(double _value1, double _value2) => Math.Max(_value1, _value2);
            protected override float NormalizedValue(double _min, double _max, double _value) => (float)((_value - _min) / (_max - _min));


            public DoubleMinMaxArray(int _dimensionSize) : base(_dimensionSize) { }
        }

        public static (float mean, float standardDeviation) EvaluateStandardDeviation(IEnumerable<float> data) {
            int count = data.Count();

            float mean = 0;
            foreach(float value in data)
                mean += value;
            mean /= count;
            
            float standardDeviation = 0;
            foreach (float value in data)
                standardDeviation += Mathf.Pow(value - mean, 2);
            standardDeviation = Mathf.Sqrt(standardDeviation / count);

            return (mean, standardDeviation);
        }
        public static IEnumerable<float> NormalizeData(IEnumerable<float> data) {
            float mean = 0;
            float standardDeviation = 0;
            (mean, standardDeviation) = EvaluateStandardDeviation(data);
            float[] normalizedData = new float[data.Count()];
            int i = 0;
            foreach (float value in data) 
                normalizedData[i++] = (value - mean) / standardDeviation;
            return normalizedData;
        }

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
        public static double FlipReference(this double value) {
            return value + 1 - 2 * value.Fraction();
        }
        public static double Fraction(this double value) {
            return (double)((decimal)value % 1);
        }
    }
}