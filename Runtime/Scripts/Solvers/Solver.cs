using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Linq;

namespace PolytopeSolutions.Toolset.Solvers {
    public abstract class Solver : MonoBehaviour {
        [Header("Core Parameters")]
        // Exposed Solver settings:
        // - whether it is live or manual update
        [SerializeField] protected bool flagLiveUpdate = false;
        // - whether it should auto-update the solution on clean up
        [SerializeField] protected bool flagAutoUpdateSolution;
        // - whether it is in debug mode (step-by-step)
        [SerializeField] private bool flagDebugging = false;
        // - randomness seed
        [SerializeField] private string seed;
        // - whether it should auto-update the seed on solution restart
        [SerializeField] private bool flagAutoUpdateSeed;
        // - container for the solution generated objects (if null won't have objects)
        [SerializeField] protected Transform tSolutionParentHolder;
        // - input smooth duration
        [SerializeField] protected float inputControllerSmoothDuration = 2f;

        // Core internal parameters:
        // - input controller handling dependencies and their smooth transition
        protected SolverInputController inputController = null;
        // - the solution itself
        protected ISolutionDescriptor solution = null;
        // - the randomizer
        protected System.Random randomizer;
        // - the gameobject holding the solution
        protected GameObject gSolutionHolder;
        // - coroutines for hangling inputs and solution selection
        private Coroutine inputUpdateCoroutine, selectSolutionCoroutine;
        // - flag for debugging (blocks solution loop until is set to true)
        private bool flagDebugNextStep = false;

        // - Exposed actions for solution events:
        //   - OnSolutionSucces: when the solution is finished successfully
        public Action OnSolutionSucces;
        //   - OnSolutionFail: when the solution is finished unsuccessfully
        public Action OnSolutionFail;
        //   - OnCleared: when the solution is cleared
        public Action OnCleared;
        //   - OnObjectsAdded: when the solution adds objects
        public Action<Dictionary<object, Transform>> OnObjectsAdded;
        //   - OnObjectsRemoved: when the solution removes objects
        public Action<Dictionary<object, Transform>> OnObjectsRemoved;

        // Additional parameters:
        // - the name of the solution holder (if is null - no object will be generated)
        protected abstract string SolutionName { get; }
        // - the solvers this solver depends on (if null - no dependencies)
        protected virtual List<Solver> DependsOnSolvers { get; } = null;
        // - exposed flag for whether the dependencies are available
        protected bool FlagDependencySolutionsAvailable {
            get {
                bool state = (this.DependsOnSolvers == null);
                if (!state)
                    this.DependsOnSolvers.ForEach(solver => 
                        state &= (solver != null || !solver.FlagSolutionAvailable)
                    );
                return state;
            }
        }
        // - the solvers this solver codepends on (if null - no codependencies)
        protected virtual List<Solver> CodependsOnSolvers { get; } = null;
        // - exposed flag for whether the solution is available
        public bool FlagSolutionAvailable => (this.solution != null && this.solution.FlagSolutionAvailable);
        // - exposed solution descriptor
        public string SolutionLog => this.solution?.SolutionLog;

        ///////////////////////////////////////////////////////////////////////
        #region UNITY_FUNCTIONS
        protected virtual void Awake() {
            InitializeInputController();
            PrepareSolve();
        }
        protected virtual void Start() {
            if (this.flagLiveUpdate) {
                if (this.selectSolutionCoroutine != null) StopCoroutine(this.selectSolutionCoroutine);
                this.selectSolutionCoroutine = StartCoroutine(ContinuousSolve());
            }
            SetCallbacks();
        }
        protected virtual void Update() {
            TickUpdate();
        }
        protected virtual void OnDestroy() {
            UnsetCallbacks();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region INPUT_HANDLING
        protected abstract void InitializeInputController();
        protected abstract void SetCallbacks();
        protected abstract void UnsetCallbacks();
        protected void TryLaunchInputUpdate() {
            if (this.flagLiveUpdate && this.inputUpdateCoroutine == null)
                this.inputUpdateCoroutine = StartCoroutine(UpdateInputs());
        }
        private IEnumerator UpdateInputs() {
            bool inputStable = false;
            do {
                OnUpdateInputs();
                this.solution.TickInput(ref inputStable, this.inputController);
                yield return null;
            } while (!inputStable);
            OnStabilizeInputs();
            #if DEBUG
            Debug.Log("Solver: InputSettled");
            #endif
            this.inputUpdateCoroutine = null;
        }
        protected virtual void OnUpdateInputs() { }
        protected virtual void OnStabilizeInputs() { }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region MAIN_HANDLING
        protected abstract void InitializeSolution();
        private void InitializeSolutionGameObject() {
            if (this.tSolutionParentHolder != null) {
                if (this.gSolutionHolder == null && !string.IsNullOrEmpty(this.SolutionName)) {
                    this.gSolutionHolder = this.tSolutionParentHolder.gameObject.TryFindOrAddByName(this.SolutionName);
                }
                //else {
                //    this.gSolutionHolder.SetActiveChildren(false);
                //}
            }
        }
        protected virtual void OnSolutionReset() { }
        private void HandleDependantSolverActivation() {
            if (this.solution.FlagSolutionActiveInternal && !this.solution.FlagSolutionActiveExternal && this.FlagDependencySolutionsAvailable) {
                this.solution.FlagSolutionActiveExternal = true;
                this.solution.FlagSolutionActiveInternal = true;
            }
            else if (this.solution.FlagSolutionActiveInternal && this.solution.FlagSolutionActiveExternal && !this.FlagDependencySolutionsAvailable)
                this.solution.FlagSolutionActiveExternal = false;
        }

        private void PrepareSolve() {
            UpdateSeed();
            InitializeSolution();
            InitializeSolutionGameObject();
            #if DEBUG
            Debug.Log("Solver: Solution is initialized.");
            #endif
        }
        [ContextMenu("SolveOneTime")]
        public virtual void OneTimeSolve() {
            OnSolutionReset();
            if (this.flagLiveUpdate) { 
                this.solution.FlagSolutionActiveInternal = true;
            }
            else { 
                PrepareSolve();
                this.solution.FlagSolutionActiveInternal = true;
                this.selectSolutionCoroutine = StartCoroutine(SelectSolutionOnce());
            }
        }
        // Select a solution.
        private IEnumerator SelectSolutionOnce() {
            #if DEBUG
            Debug.Log("Solver: Solve");
            #endif
            // Wait for inputs.
            if (this.inputUpdateCoroutine != null) StopCoroutine(this.inputUpdateCoroutine);
            this.inputUpdateCoroutine = StartCoroutine(UpdateInputs());
            yield return this.inputUpdateCoroutine;

            // Solve.
            bool finished = false;
            DateTime tempTimeStart = DateTime.Now;
            int cycleCount = 0;
            do {
                HandleDependantSolverActivation();
                if (this.flagDebugging) {
                    while (!this.flagDebugNextStep)
                        yield return null;
                    this.flagDebugNextStep = false;
                }
                else
                    yield return null;
                finished = this.solution.TickMain(this.randomizer, this.inputController);
                cycleCount++;
            } while (!finished);
            #if DEBUG2
            Debug.Log("Solver: Selection: Time for Solver: " + (DateTime.Now - tempTimeStart).TotalSeconds.ToString("F3") 
                + ". cycles attemted: " + cycleCount);
            #endif
            FinishSolve();
            yield return null;
        }
        private IEnumerator ContinuousSolve() { 
            yield return null;
            #if DEBUG
            Debug.Log("Solver: Starting Continuous Solve");
            #endif
            this.solution.FlagSolutionActiveInternal = true;
            bool wasFinished = false;
            while (true) {
                HandleDependantSolverActivation();
                if (this.flagDebugging) {
                    while (!this.flagDebugNextStep)
                        yield return null;
                    this.flagDebugNextStep = false;
                } else 
                    yield return null;

                bool finished = this.solution.TickMain(this.randomizer, this.inputController);
                if (!wasFinished && finished)
                    FinishSolve();
                wasFinished = finished;
                yield return null;
            }
            //yield return null;
        }
        private void FinishSolve() {
            #if DEBUG
            Debug.Log("Solver: Solution Finished: " + this.solution.SolutionLog);
            #endif
            if (this.solution.FlagSolutionSuccess) 
                this.OnSolutionSucces?.Invoke();
            else 
                this.OnSolutionFail?.Invoke();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region CLEANUP_HANDLING
        [ContextMenu("ClearAndRestart")]
        public virtual void ClearAndTryRestart() {
            OneTimeClear();
            if (this.FlagDependencySolutionsAvailable && this.flagAutoUpdateSolution)
                Invoke("Solve", 0.05f); // Delay to allow for cleanup.
        }
        public virtual void OneTimeClear() {
            if (!this.flagLiveUpdate && this.selectSolutionCoroutine != null) 
                StopCoroutine(this.selectSolutionCoroutine);

            if (this.solution != null)
                this.solution.ClearSolution();
            //if (this.gSolutionHolder != null)
            //    this.gSolutionHolder.DestroyChildren();

            FinishClear();
        }
        private void FinishClear() {
            if (this.OnCleared != null)
                this.OnCleared.Invoke();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region UPDATE_HANDLING
        protected virtual void TickUpdate() {
            if (!this.solution.FlagSolutionUpdated)
                return;
            #if DEBUG2
            Debug.Log("Solver: Updated: " + this.solution.FlagSolutionUpdated);
            #endif

            this.solution.TickUpdate(this.randomizer, this.inputController, this.gSolutionHolder,
                out Dictionary<object, Transform> newObjects, out Dictionary<object, Transform> removeObjects);
            if (newObjects.Count > 0)
                this.OnObjectsAdded?.Invoke(newObjects);
            if (removeObjects.Count > 0)
                this.OnObjectsRemoved?.Invoke(removeObjects);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region MISC_FUNCTIONS
        public void SetSeed(string _seed) {
            this.seed = _seed;
            this.flagAutoUpdateSeed = false;
        }
        protected void UpdateSeed(string _seed = null) {
            if (this.flagAutoUpdateSeed) { 
                this.seed = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss:fff");
            }
            this.randomizer = new System.Random(this.seed.GetHashCode());
        }
        public bool FlagDebugging => this.flagDebugging;
        public void DebugNextStep() {
            this.flagDebugNextStep = true;
        }
        #endregion
    }
}