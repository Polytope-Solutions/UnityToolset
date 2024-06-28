using UnityEngine;
using System.Collections;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public class MeshGameObject {
        public GameObject goItem { get; private set; }
        public Transform tItem { get; private set; }

        public MeshFilter mfDefaultMesh { get; private set; }
        public MeshRenderer mrDefaultMesh { get; private set; }
        public MeshCollider mcMesh { get; private set; }
        private Material material;
        public Material Material { 
            get => this.material;
            set {
                this.material = value;
                this.mrDefaultMesh.sharedMaterial = this.material;            
            }
        }
        public Texture2D mainTexture {
            set { 
                this.material.mainTexture = value;
            }
        }

        private bool shareCollisionMesh;
        private Mesh mainMesh;
        public Mesh MainMesh { 
            get => this.mainMesh; 
            set {
                this.mainMesh = value;
                this.mfDefaultMesh.sharedMesh = value;
                if (this.shareCollisionMesh)
                    this.CollisionMesh = value;
            } 
        }
        private Mesh collisionMesh;
        public Mesh CollisionMesh {
            get => this.collisionMesh;
            set {
                if (!this.mcMesh) return;
                this.collisionMesh = value;
                this.mcMesh.sharedMesh = value;
                if (this.shareCollisionMesh) {
                    this.mainMesh = value;
                    this.mfDefaultMesh.sharedMesh = value;
                }
            }
        }

        public MeshGameObject(GameObject goParent, string name, GameObject goPrefab, Material _material=null, 
                int? layer = null, bool instantiateMaterial=false, bool useColliders = false, bool shareCollisionMesh=false) {
            goParent.SetActive(false);
            this.goItem = goParent.TryFindOrAddByName(name, goPrefab);
            if (layer != null)
                this.goItem.layer = layer.Value;
            this.tItem = this.goItem.transform;
            this.tItem.position = Vector3.zero;
            this.tItem.localRotation = Quaternion.identity;
            this.tItem.localScale = Vector3.one;

            this.shareCollisionMesh = shareCollisionMesh;
            this.mfDefaultMesh = this.goItem.GetComponent<MeshFilter>();
            if (!this.mfDefaultMesh)
                this.mfDefaultMesh = this.goItem.AddComponent<MeshFilter>();
            this.mrDefaultMesh = this.goItem.GetComponent<MeshRenderer>();
            if (!this.mrDefaultMesh)
                this.mrDefaultMesh = this.goItem.AddComponent<MeshRenderer>();
            if (useColliders) { 
                this.mcMesh = this.goItem.GetComponent<MeshCollider>();
                if (!this.mcMesh)
                    this.mcMesh = this.goItem.AddComponent<MeshCollider>();
            }

            if (_material == null)
                _material = this.mrDefaultMesh.sharedMaterial;
            if (instantiateMaterial)
                this.Material = Material.Instantiate(_material);
            else
                this.Material = _material;

            this.MainMesh = new Mesh();
            if (useColliders && !this.shareCollisionMesh)
                this.CollisionMesh = new Mesh();
            
            goParent.SetActive(true);
        }
        public void UploadMeshDataToGPU() {
            //this.MainMesh.UploadMeshData(true);
            //if (this.mcMesh && !this.shareCollisionMesh)
            //    this.CollisionMesh.UploadMeshData(true);
        }
        public IEnumerator UploadMeshDataToGPUCoroutine() {
            yield return null;
            UploadMeshDataToGPU();
        }
        public void SetActive(bool targetState) {
            this.goItem.SetActive(targetState);
        }
    }
}
