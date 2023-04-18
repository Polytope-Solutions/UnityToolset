using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Solvers {
    public class Solver : MonoBehaviour {
        public bool flagForceResolve;
        public bool flagAutoUpdateSolution;
        public bool flagAutoUpdateSeed;
        protected bool flagSolutionChanged;
        protected bool flagSolutionUpdated;
        protected bool flagSolutionSuccess;

        public bool flagSolutionAvailable => !this.flagSolutionSuccess;

        public Transform tSolutionParentHolder;
        protected GameObject gSolutionHolder;

        protected virtual string solutionName {
            get {
                return "SolutionHolder";
            }
        }

        public string seed;
        protected System.Random randomizer;

        public delegate void onSolveEvent();
        public onSolveEvent OnSolutionSucces;
        public onSolveEvent OnSolutionFail;
        public onSolveEvent OnCleared;

        ///////////////////////////////////////////////////////////////////////////////////
        [ContextMenu("Solve")]
        public virtual void Solve() {}
        protected virtual void PrepareSolve() {
            this.flagSolutionChanged = false;
            this.flagSolutionUpdated = false;
            this.flagSolutionSuccess = false;
            UpdateSeed();
        }
        protected void FinishSolve() {
            if (this.flagSolutionSuccess) { 
                if (this.OnSolutionSucces != null)
                    this.OnSolutionSucces.Invoke();
            }
            else { 
                if (this.OnSolutionFail != null)
                    this.OnSolutionFail.Invoke();
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////
        [ContextMenu("Clear")]
        public virtual void Clear() {
            this.gSolutionHolder.DestroyChildren();
            FinishClear();
            if (this.flagAutoUpdateSolution)
                Solve();
        }
        protected void FinishClear() {
            if (this.OnCleared != null)
                this.OnCleared.Invoke();
        }

        ///////////////////////////////////////////////////////////////////////////////////
        protected void PrepareSolutionStructure() {
            if (this.tSolutionParentHolder != null) { 
                if (this.gSolutionHolder == null) {
                    this.gSolutionHolder = this.tSolutionParentHolder.gameObject.TryFind(this.solutionName);
                }
                //else {
                //    this.gSolutionHolder.SetActiveChildren(false);
                //}
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////
        protected void UpdateSeed() {
            if (this.flagAutoUpdateSeed) { 
                this.seed = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss:fff");
            }
            this.randomizer = new System.Random(this.seed.GetHashCode());
        }
    }
}