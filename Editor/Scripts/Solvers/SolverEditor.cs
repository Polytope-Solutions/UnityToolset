using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using PolytopeSolutions.Toolset.Solvers;

namespace PolytopeSolutions.Toolset.Editor.Solvers {
    [CustomEditor(typeof(Solver), true)]
    [CanEditMultipleObjects]
    public class SolverEditor : UnityEditor.Editor {
        private Solver targetObject;
        void OnEnable() {
            this.targetObject = (Solver)serializedObject.targetObject;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("Solve")) { 
                this.targetObject.flagForceResolve = true;
                this.targetObject.Solve();
            }
            if (GUILayout.Button("Clear"))
                this.targetObject.Clear();
        }
    }
}