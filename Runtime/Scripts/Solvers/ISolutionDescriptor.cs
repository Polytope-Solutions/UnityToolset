using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Linq;

namespace PolytopeSolutions.Toolset.Solvers {
    public interface ISolutionDescriptor {
        ///////////////////////////////////////////////////////////////////////
        #region INTERFACE_CORE
        // Flag for whether the solution needs update tick.
        public bool FlagSolutionUpdated { get; }
        // Flag for whether the solution is available.
        public bool FlagSolutionAvailable { get; }
        // Flag for whether the solution is successful.
        public bool FlagSolutionSuccess { get; }
        // Descriptor of solution state
        public string SolutionLog { get; }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region CLEANUP_HANDLING
        // Clear the solution.
        public virtual void ClearSolution() {
            DeactivateSolutionInternal();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region ACTIVATIONS
        protected bool FlagSolutionActiveTotal => this.FlagSolutionActiveInternal && this.FlagSolutionActiveExternal;
        public bool FlagSolutionActiveInternal { get; set; }
        public bool FlagSolutionActiveExternal { get; set; }
        public virtual void ActivateSolutionInternal() {
            this.FlagSolutionActiveInternal = true;
        }
        public void ActivateSolutionExternal() {
            this.FlagSolutionActiveExternal = true;
            ActivateSolutionInternal();
        }
        public void DeactivateSolutionInternal() {
            this.FlagSolutionActiveInternal = false;
        }
        public void DeactivateSolutionExternal() {
            this.FlagSolutionActiveExternal = false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region INPUT_HANDLING
        // Exposed trigger for input tick.
        public void TickInput(ref bool inputStable, SolverInputController inputController) {
            if (inputController.IsComplete) {
                #if DEBUG2
                Debug.Log("Solution: InputTick.");
                #endif
                HandleInputTick(inputController);

                inputStable = inputController.TickInputs();
            }
        }
        // Required overridable function to handle input tick.
        protected void HandleInputTick(SolverInputController inputController);
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region MAIN_HANDLING
        // Exposed trigger for main tick.
        public bool TickMain(System.Random randomizer, SolverInputController inputController) {
            if (inputController.IsComplete) {
                #if DEBUG2
                Debug.Log("Solution: MainTick.");
                #endif
                return HandleMainTick(randomizer, inputController);
            }
            return false;
        }
        // Required overridable function to handle main tick.
        protected bool HandleMainTick(System.Random randomizer, SolverInputController inputController);
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region UPDATE_HANDLING
        // Exposed trigger for update tick.
        public void TickUpdate(System.Random randomizer, SolverInputController inputController, GameObject gSolutionHolder,
                out Dictionary<object, Transform> newObjects, out Dictionary<object, Transform> removeObjects) {
            newObjects = new Dictionary<object, Transform>();
            removeObjects = new Dictionary<object, Transform>();
            if (inputController.IsComplete) {
                #if DEBUG2
                Debug.Log("Solution: UpdateTick.");
                #endif
                HandleUpdateTick(randomizer, inputController, gSolutionHolder, ref newObjects, ref removeObjects);
            }
        }
        // Required overridable function to handle update tick.
        protected void HandleUpdateTick(System.Random randomizer, SolverInputController inputController, GameObject gSolutionHolder,
                ref Dictionary<object, Transform> newObjects, ref Dictionary<object, Transform> removeObjects);
        #endregion
    }
}