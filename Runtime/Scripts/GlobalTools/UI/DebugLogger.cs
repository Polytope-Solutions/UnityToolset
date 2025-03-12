using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Text;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
    //[ExecuteAlways]
    public class DebugLogger : MonoBehaviour {
        [SerializeField] private int logCount = 20;
        [SerializeField] private bool debugEnabledOnly = true;
        private string[] logs;
        private int logIndex;
        private bool isBufferFull, isUpdated;
        private StringBuilder logStringBuilder;

        private void Awake() {
            if (!this.debugEnabledOnly)
                Init();
        }
        private void OnEnable() {
            if (this.debugEnabledOnly)
                Init();
        }
        private void OnDisable() {
            if (this.debugEnabledOnly)
                Application.logMessageReceivedThreaded -= LogMessageReceived;
        }
        private void OnDestroy() {
            if (!this.debugEnabledOnly)
                Application.logMessageReceivedThreaded -= LogMessageReceived;
        }
        private void Update() {
            if (this.isUpdated)
                lock (this.logStringBuilder) {
                    string message = UpdateText();
                    gameObject.SetText(message);
                }
            this.isUpdated = false;
        }

        private void Init() {
            this.logs = new string[logCount];
            this.logIndex = 0;
            this.isBufferFull = false;
            this.logStringBuilder = new StringBuilder();
            Application.logMessageReceivedThreaded += LogMessageReceived;
        }

        private void LogMessageReceived(string logString, string stackTrace, LogType type) {
            if (this.logIndex + 1 == this.logCount)
                this.isBufferFull = true;
            this.logs[this.logIndex]
                = $"{((type != LogType.Log) ? $"<b>{type}|" : "")}{logString}{((type != LogType.Log) ? $"</b>" : "")}";
            this.logIndex = (this.logIndex + 1) % this.logCount;
            this.isUpdated = true;
        }

        private string UpdateText() {
            lock (this.logStringBuilder) {
                this.logStringBuilder.Clear();
                if (this.isBufferFull)
                    for (int i = 0, j = this.logIndex; i < this.logCount; i++, j++) {
                        this.logStringBuilder.AppendLine(this.logs[j]);
                        if (j + 1 == this.logCount)
                            j -= this.logCount;
                    }
                else
                    for (int i = 0; i < this.logIndex; i++)
                        this.logStringBuilder.AppendLine(this.logs[i]);
            }
            return this.logStringBuilder.ToString();
        }
    }
}