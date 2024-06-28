using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using PolytopeSolutions.Toolset.GlobalTools.Types;
using System.Threading.Tasks;
using System;
using NUnit.Framework.Internal;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public class MeshManager : TManager<MeshManager> {
        [SerializeField] private int maxMeshesPerFrame = 4;
        //[SerializeField] private int objectsToBatchLimit = 50;
        //[SerializeField] private int framesToWaitForBatching = 30;
        private Queue<(Task<(Mesh, Mesh, Texture2D)> task, MeshGameObject meshGameObject, Action<MeshGameObject> callback, GameObject goBatchRoot)> queue;
        //private Dictionary<GameObject, List<GameObject>> objectsToBatch;
        //private int framesWithoutUpdates;

        protected override void Awake() {
            base.Awake();
            this.queue = new Queue<(Task<(Mesh, Mesh, Texture2D)>, MeshGameObject, Action<MeshGameObject>, GameObject goBatchRoot)>();
            //this.objectsToBatch = new Dictionary<GameObject, List<GameObject>>();
            //this.framesWithoutUpdates = this.framesToWaitForBatching;
        }

        public void QueueUp(Task<(Mesh, Mesh, Texture2D)> task, MeshGameObject meshGameObject, Action<MeshGameObject> callback=null, GameObject goBatchRoot=null) { 
            this.queue.Enqueue((task, meshGameObject, callback, goBatchRoot));
        }
        private async Awaitable Update() {
            int meshesProccessed = 0;
            bool batchUpdate = false;
            while (this.queue.Count > 0 && meshesProccessed < this.maxMeshesPerFrame) {
                (Task<(Mesh, Mesh, Texture2D)> task, MeshGameObject meshGameObject, Action<MeshGameObject> callback, GameObject goBatchRoot) = this.queue.Dequeue();

                (Mesh mesh, Mesh collisionMesh, Texture2D texture) = await task;

                if (mesh)
                    meshGameObject.MainMesh = mesh;
                if (collisionMesh)
                    meshGameObject.CollisionMesh = collisionMesh;
                if (texture)
                    meshGameObject.mainTexture = texture;

                //if (goBatchRoot) {
                //    if (!this.objectsToBatch.ContainsKey(goBatchRoot))
                //        this.objectsToBatch.Add(goBatchRoot, new List<GameObject>());
                //    this.objectsToBatch[goBatchRoot].Add(meshGameObject.goItem);
                //    batchUpdate = true;
                //}

                callback?.Invoke(meshGameObject);
                meshesProccessed++;
            }
            //if (!batchUpdate && this.framesWithoutUpdates >= 0)
            //    this.framesWithoutUpdates--;
            //else { 
            //    List<GameObject> toRemove = new List<GameObject>();
            //    foreach (var batchGroup in this.objectsToBatch) {
            //        if (batchGroup.Value.Count > this.objectsToBatchLimit || this.framesWithoutUpdates < 0) { 
            //            StaticBatchingUtility.Combine(batchGroup.Value.ToArray(), batchGroup.Key);
            //            //toRemove.Add(batchGroup.Key);
            //        }
            //    }
            //    //for (int i = toRemove.Count-1; i >= 0; i--) 
            //    //    this.objectsToBatch.Remove(toRemove[i]);
            //    this.framesWithoutUpdates = this.framesToWaitForBatching;
            //}
        }
    }
}