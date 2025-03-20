using UnityEngine;
using System.Collections;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public class MeshGameObject {
        public GameObject goItem { get; private set; }
        public Transform tItem { get; private set; }
        public string name => this.goItem.name;

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
        private Texture2D mainTexture;
        public Texture2D MainTexture {
            set {
                this.mainTexture = value;
                this.material.mainTexture = this.mainTexture;
            }
            get {
                return this.mainTexture;
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

        public MeshGameObject(GameObject goParent, string name, GameObject goPrefab,
                int? layer = null, bool useColliders = false, bool shareCollisionMesh = false) {
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

            //this.MainMesh = new Mesh();
            //if (useColliders && !this.shareCollisionMesh)
            //    this.CollisionMesh = new Mesh();

            goParent.SetActive(true);
        }
        public MeshGameObject(GameObject goParent, string name, GameObject goPrefab, Material _material = null,
                int? layer = null, bool instantiateMaterial = false, bool useColliders = false, bool shareCollisionMesh = false)
                : this(goParent, name, goPrefab, layer, useColliders, shareCollisionMesh) {
            if (_material == null)
                _material = this.mrDefaultMesh.sharedMaterial;
            if (instantiateMaterial)
                this.Material = Material.Instantiate(_material);
            else
                this.Material = _material;
        }
        public void Destroy() {
#if UNITY_EDITOR
            if (this.mainMesh)
                Mesh.DestroyImmediate(this.mainMesh, true);
            if (this.collisionMesh)
                Mesh.DestroyImmediate(this.collisionMesh, true);
            if (this.mainTexture)
                Texture2D.DestroyImmediate(this.mainTexture, true);
            if (this.material)
                Material.DestroyImmediate(this.material, true);
            if (this.goItem)
                GameObject.DestroyImmediate(this.goItem, true);
#else
            if (this.mainMesh)
                Mesh.Destroy(this.mainMesh);
            if (this.collisionMesh)
                Mesh.Destroy(this.collisionMesh);
            if (this.mainTexture)
                Texture2D.Destroy(this.mainTexture);
            if (this.material)
                Material.Destroy(this.material);
            if (this.goItem)
                GameObject.Destroy(this.goItem);
#endif
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
