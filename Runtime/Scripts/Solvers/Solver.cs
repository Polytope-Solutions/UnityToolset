using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolytopeSolutions.Toolset.Solvers {
    public class Solver : MonoBehaviour {
        public bool flagForceResolve;
        public bool flagAutoUpdateSolution;
        public bool flagAutoUpdateSeed;
        protected bool flagSolutionChanged;
        protected bool flagSolutionUpdated;

        public string seed;
        protected System.Random randomizer;
        
        [ContextMenu("Solve")]
        public virtual void Solve() { }
        [ContextMenu("Clear")]
        public virtual void Clear() { }

        protected void UpdateSeed() {
            if (this.flagAutoUpdateSeed) { 
                this.seed = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss:fff");
            }
            this.randomizer = new System.Random(this.seed.GetHashCode());
        }
    }
}