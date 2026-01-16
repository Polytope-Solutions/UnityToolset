using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
	[ExecuteAlways]
	public class UIFPS : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI averageFPSLabel;
        [SerializeField] private TextMeshProUGUI lowestFPSLabel;
        [Range(0,6)]
		[SerializeField] private int precision = 3;
		[SerializeField] private bool unscaledDeltaTime = false;

		private float[] buffer;
		[SerializeField] private int bufferSize = 10;
		private int currentIndex = 0;
		private bool filled;
		private float averageFPS {
			get {
				float sum = 0;
				int count = (this.filled) ? this.bufferSize : this.currentIndex;
				for (int i = 0; i < count; i++)
                    sum += this.buffer[i];
				return sum/count;
			}
		}
		private float lowestFPS {
            get {
                float lowest = float.MaxValue;
                int count = (this.filled) ? this.bufferSize : this.currentIndex;
                for (int i = 0; i < count; i++)
                    lowest = Mathf.Min(lowest, this.buffer[i]);
                return lowest;
            }
        }

		private void OnEnable() {
			Initizalize();
		}
		private void OnValidate() {
			Initizalize();
		}
        private void Initizalize() {
			if (!this.averageFPSLabel)
				this.averageFPSLabel = gameObject.TryGetOrAddComponent<TextMeshProUGUI>();
			this.buffer = new float[this.bufferSize];
			this.currentIndex = 0;
			this.filled = false;
        }

		private void Update() {
			if (this.buffer != null) {
				float value = 1.0f / ((this.unscaledDeltaTime) ? Time.unscaledDeltaTime : Time.deltaTime);
				this.buffer[this.currentIndex] = value;
				this.currentIndex = (this.currentIndex + 1) % this.bufferSize;
				if (this.currentIndex == 0) this.filled = true;

				this.averageFPSLabel.text = this.averageFPS.ToString($"F{this.precision}");
                if (this.lowestFPSLabel)
					this.lowestFPSLabel.text = this.lowestFPS.ToString($"F{this.precision}");
            }
		}
	}
}