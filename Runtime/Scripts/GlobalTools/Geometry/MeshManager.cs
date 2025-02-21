using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public abstract class MeshManager<TTask, TIdentifier, TResult> : TManager<MeshManager<TTask, TIdentifier, TResult>> {
        [SerializeField] protected int maxMeshesPerFrame = 4;
        protected Queue<(TTask task, TIdentifier identifier, Action<TIdentifier, List<TResult>> callback)> queue;

        //[SerializeField] private int objectsToBatchLimit = 50;
        //private Dictionary<GameObject, List<GameObject>> objectsToBatch;
        //[SerializeField] private int framesToWaitForBatching = 30;
        //private int framesWithoutUpdates;

        protected override void Awake() {
            base.Awake();
            this.queue = new Queue<(TTask task, TIdentifier identifier, Action<TIdentifier, List<TResult>> callback)>();
            //this.objectsToBatch = new Dictionary<GameObject, List<GameObject>>();
            //this.framesWithoutUpdates = this.framesToWaitForBatching;
        }

        public void QueueUp(TTask task, TIdentifier identifier, Action<TIdentifier, List<TResult>> callback) {
            lock (this.queue) {
                this.queue.Enqueue((task, identifier, callback));
            }
        }
    }
}