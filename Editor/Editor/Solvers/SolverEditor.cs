using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using PolytopeSolutions.Toolset.Solvers;

namespace PolytopeSolutions.Toolset.Editor.Solvers {
    [CustomEditor(typeof(Solver), true)]
    [CanEditMultipleObjects]
    public class SolverEditor : UnityEditor.Editor {
        protected Solver targetObject;
        void OnEnable() {
            this.targetObject = (Solver)serializedObject.targetObject;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("OneTimeSolve")) { 
                this.targetObject.OneTimeSolve();
            }
            if (GUILayout.Button("OneTimeClear"))
                this.targetObject.OneTimeClear();
        }
    }
}