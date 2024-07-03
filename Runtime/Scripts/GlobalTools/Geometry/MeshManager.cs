using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public abstract class MeshManager<T> : TManager<MeshManager<T>> {
        [SerializeField] protected int maxMeshesPerFrame = 4;
        protected Queue<(T task, MeshGameObject meshGameObject, Action<MeshGameObject> callback, GameObject goBatchRoot)> queue;

        //[SerializeField] private int objectsToBatchLimit = 50;
        //private Dictionary<GameObject, List<GameObject>> objectsToBatch;
        //[SerializeField] private int framesToWaitForBatching = 30;
        //private int framesWithoutUpdates;

        protected override void Awake() {
            base.Awake();
            this.queue = new Queue<(T, MeshGameObject, Action<MeshGameObject>, GameObject goBatchRoot)>();
            //this.objectsToBatch = new Dictionary<GameObject, List<GameObject>>();
            //this.framesWithoutUpdates = this.framesToWaitForBatching;
        }

        public void QueueUp(T task, MeshGameObject meshGameObject, Action<MeshGameObject> callback = null, GameObject goBatchRoot = null) {
            lock (this.queue) {
                this.queue.Enqueue((task, meshGameObject, callback, goBatchRoot));
            }
        }
    }
}