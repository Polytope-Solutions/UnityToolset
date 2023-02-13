using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
	[ExecuteAlways]
	public class DebugLogger : MonoBehaviour {
		private List<string> logs;
		public int logCount = 20;
		// Start is called before the first frame update
		void OnEnable() {
			this.logs = new List<string>();
			UpdateText();
			Application.logMessageReceivedThreaded += logMessageReceived;
		}
		void OnDisable(){
			Application.logMessageReceivedThreaded -= logMessageReceived;
		}
        
		void logMessageReceived(string logString, string stackTrace, LogType type) {
			while (this.logs.Count > this.logCount)
				this.logs.RemoveAt(0);
			this.logs.Add(logString);
			UpdateText();
		}
        
		private void UpdateText(){
			gameObject.SetText(string.Join("\n", this.logs));
		}
	}
}