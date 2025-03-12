using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine.Events;

namespace PolytopeSolutions.Toolset.Input {
    public abstract class ExposedActionInputHandler<T> : MonoBehaviour, IInputHandler where T : InputReceiver {
        [SerializeField] protected T inputReceiver;
        InputReceiver IInputHandler.InputReceiver {
            get {
                if (!this.inputReceiver)
                    this.inputReceiver = GameObject.FindFirstObjectByType<T>();
                return this.inputReceiver;
            }
        }

        [SerializeField] protected UnityEvent onInteractionStarted;
        [SerializeField] protected UnityEvent<RaycastHit> onInteractionPerformed;
        [SerializeField] protected UnityEvent onInteractionEnded;

        protected virtual void Start() {
            ((IInputHandler)this).StartInputHandler();
        }
        protected virtual void OnDestroy() {
            ((IInputHandler)this).OnDestroyInputHandler();
        }
        ///////////////////////////////////////////////////////////////////////
        // Methods to be called by the input receiver,
        bool IInputHandler.IsRelevantHandler(object data) {
            return IsRelevantHandler((RaycastHit)data);
        }
        // if at the moment of press, this handler is being hovered over
        void IInputHandler.OnInteractionStarted() { 
            #if DEBUG2
            this.Log($"Interaction started");
            #endif
            this.onInteractionStarted?.Invoke();
        }
        void IInputHandler.OnInteractionPerformed(object data) { 
            #if DEBUG2
            this.Log($"Ray hit: {hitinfo.transform.gameObject.name}");
            #endif
            this.onInteractionPerformed?.Invoke((RaycastHit)data);
        }
        void IInputHandler.OnInteractionEnded() {
            #if DEBUG2
            this.Log($"Interaction ended");
            #endif
            this.onInteractionEnded?.Invoke();
        }
        protected abstract bool IsRelevantHandler(RaycastHit ray);
    }
}