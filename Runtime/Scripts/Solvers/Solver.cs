using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Solvers {
    public class Solver : MonoBehaviour {
        public bool flagForceResolve;
        public bool flagAutoUpdate;
        protected bool flagSolutionChanged;
        protected bool flagSolutionUpdated;
        
        [ContextMenu("Solve")]
        public virtual void Solve() { }
        [ContextMenu("Clear")]
        public virtual void Clear() { }
    }
}