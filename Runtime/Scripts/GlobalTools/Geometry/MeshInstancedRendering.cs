using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public static partial class MeshTools {
        public class MeshObjectInstanceData {
            private class MeshInstanceData {
                public Matrix4x4 internalTransformation;
                public Mesh mesh;
                public Material material;
                public RenderParams renderParams;

                public List<MeshInstanceItemDataCluster> clusters;
                public class MeshInstanceItemDataCluster {
                    public struct MeshItemData {
                        public Matrix4x4 objectToWorld;
                    }
                    public MeshItemData[] data;
                    public int itemCount;
                    private static readonly int maxDataCount = 500;
                    public MeshInstanceItemDataCluster() {
                        this.data = new MeshItemData[maxDataCount];
                        this.itemCount = 0;
                    }
                    public bool ExceededCapacity => this.itemCount >= maxDataCount;
                    public void Add(Matrix4x4 objectToWorld) {
                        this.data[this.itemCount++].objectToWorld = objectToWorld;
                    }
                }

                private MeshInstanceData(Mesh mesh, Material material, bool castShadows = true, bool receiveShadows = true) {
                    this.mesh = mesh;
                    this.material = material;
                    this.renderParams = new RenderParams(this.material);
                    this.renderParams.shadowCastingMode =
                        (castShadows) ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    this.renderParams.receiveShadows = receiveShadows;
                    this.clusters = new List<MeshInstanceItemDataCluster>();
                }
                public MeshInstanceData(MeshFilter meshFilter) : this(meshFilter.sharedMesh, meshFilter.GetComponent<MeshRenderer>().sharedMaterial) { }
                public void Add(Matrix4x4 objectToWorld) {
                    if (this.clusters.Count == 0)
                        this.clusters.Add(new MeshInstanceItemDataCluster());
                    if (this.clusters[this.clusters.Count - 1].ExceededCapacity)
                        this.clusters.Add(new MeshInstanceItemDataCluster());

                    this.clusters[this.clusters.Count - 1].Add(objectToWorld);
                }

                public void Render() {
                    if (this.clusters.Count == 0) return;
                    foreach (MeshInstanceItemDataCluster cluster in this.clusters) {
                        if (cluster.itemCount == 0) continue;
                        Graphics.RenderMeshInstanced(this.renderParams, this.mesh, 0, cluster.data, cluster.itemCount);
                    }
                }
            }
            private List<MeshInstanceData> instanceData;
            public MeshObjectInstanceData() {
                this.instanceData = new List<MeshInstanceData>();
            }
            public void Initialize(GameObject goHolder) {
                MeshFilter[] meshFilters = goHolder.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter meshFilter in meshFilters) {
                    if (!this.instanceData.Exists(data => data.mesh == meshFilter.sharedMesh)) {
                        this.instanceData.Add(new MeshInstanceData(meshFilter));
                    }
                }
            }
            public void Add(Matrix4x4 objectToWorld) {
                foreach (MeshInstanceData data in this.instanceData) {
                    data.Add(objectToWorld);
                }
            }
            public void Render() {
                if (this.instanceData == null || this.instanceData.Count == 0) return;
                foreach (MeshInstanceData data in this.instanceData) {
                    data.Render();
                }
            }
        }
    }
}