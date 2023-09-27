using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

using TMPro;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
	[ExecuteAlways]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class UIFPS : MonoBehaviour {
		private TextMeshProUGUI label;
		[Range(0,6)]
		[SerializeField] private int precision = 3;

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

		private void OnEnable() {
			Initizalize();
		}
		private void OnValidate() {
			Initizalize();
		}
        private void Initizalize() {
			this.label = GetComponent<TextMeshProUGUI>();
			this.buffer = new float[this.bufferSize];
			this.currentIndex = 0;
			this.filled = false;
        }

		private void Update() {
			if (this.buffer != null) {
				float value = 1.0f / Time.deltaTime;
				this.buffer[this.currentIndex] = value;
				this.currentIndex = (this.currentIndex + 1) % this.bufferSize;
				if (this.currentIndex == 0) this.filled = true;

				this.label.text = this.averageFPS.ToString("F" + this.precision);
			}
		}
	}
}