using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
	public static class MeshTools {
		public static Mesh JoinMeshes(Transform[] origins, Mesh[] sources) {
			Mesh mesh = new Mesh();

			List<Vector3> vertices = new List<Vector3>(), currentVertices = new List<Vector3>(),
                normals = new List<Vector3>(), currentNormals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>(), currentUVs = new List<Vector2>();
            List<int> indices = new List<int>(), currentIndices = new List<int>(); ;

			int currentOffset = 0;
			for (int i = 0; i < sources.Length; i++) {
                currentVertices.Clear();
				sources[i].GetVertices(currentVertices);
                currentVertices.ForEach(vertex => vertices.Add(origins[i].TransformPoint(vertex)));

				currentNormals.Clear();
                sources[i].GetNormals(currentNormals);
                currentNormals.ForEach(normal => normals.Add(origins[i].TransformDirection(normal)));

                // TODO: Add support for multiple channels
                //for (int c = 0; c < 8; c++)
                int c = 0;
                {
                    currentUVs.Clear();
                    sources[i].GetUVs(c, currentUVs);
					if (currentUVs.Count == 0)
						currentUVs = (new Vector2[sources[i].vertexCount]).ToList();
					uvs.AddRange(currentUVs);
				}

                for (int sm = 0; sm < sources[i].subMeshCount; sm++) {
					currentIndices.Clear();
					sources[i].GetTriangles(currentIndices, sm);
                    currentIndices.ForEach(index => indices.Add(index + currentOffset));
				}

				currentOffset += sources[i].vertexCount;
			}

			if (vertices.Count >= 65536)
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
			if (normals.Count == 0)
				mesh.RecalculateNormals();
            else
                mesh.SetNormals(normals);
            //for (int c = 0; c < 8; c++)
            //if (uvs[c] != null && uvs[c].Count > 0)
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();

			return mesh;
		}
		
        // Generates a square regular grid mesh with the specified resolution and size.
        // By default in XY plane, centered at origin, but has Matrix parameter if needs to be modified.
        public static Mesh RegularSquareGridMesh(int resolution, float size,
            Matrix4x4 modifier = default(Matrix4x4),
            bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            return RegularGridMesh(resolution, resolution, size, size,
                modifier, computeUVs, computeNormals, computeBounds, isMeshFinal);
        }
        // Generates a regular grid mesh with the specified resolution and size.
        // By default in XY plane, centered at origin, but has Matrix parameter if needs to be modified.
        public static Mesh RegularGridMesh(int resolutionX, int resolutionY, float sizeX, float sizeY, 
            Matrix4x4 modifier = default(Matrix4x4), 
            bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            if (modifier == default(Matrix4x4)) modifier = Matrix4x4.identity;
            Mesh mesh = new Mesh();

            int vertexCount = (resolutionX + 1) * (resolutionY + 1);
            int triangleCount = resolutionX * resolutionY * 2;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[triangleCount * 3];
            Vector3 bottomLeft = new Vector3(-sizeX / 2, 0, -sizeY / 2);

            for(int y = 0; y <= resolutionY; y++) {
                for (int x = 0; x <= resolutionX; x++) {
                    int i = x + y * (resolutionX + 1);
                    Vector2 uv = new Vector2(x / (float)resolutionX, y / (float)resolutionY);
                    vertices[i] = modifier.MultiplyPoint3x4(bottomLeft + 
                        new Vector3(uv.x*sizeX, 0, uv.y*sizeY));
                    if (computeUVs) 
                        uvs[i] = uv;
                }
            }
            for (int y = 0; y < resolutionY; y++) {
                for (int x = 0; x < resolutionX; x++) {
                    int i = (x + y * resolutionX) * 6;
                    triangles[i + 0] = x + y * (resolutionX + 1);
                    triangles[i + 1] = x + (y + 1) * (resolutionX + 1);
                    triangles[i + 2] = x + 1 + y * (resolutionX + 1);
                    triangles[i + 3] = x + 1 + y * (resolutionX + 1);
                    triangles[i + 4] = x + (y + 1) * (resolutionX + 1);
                    triangles[i + 5] = x + 1 + (y + 1) * (resolutionX + 1);
                }
            }
            mesh.indexFormat = (vertexCount > 65535) ? 
                UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            if (computeUVs) mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            if (computeNormals) mesh.RecalculateNormals();
            if (computeBounds) mesh.RecalculateBounds();
            mesh.UploadMeshData(isMeshFinal);
            return mesh;
        }
        public static void SetMeshHeights(this Mesh mesh,
            int meshResolution, Texture2D heightMap,
            Vector2 heightRange,
            bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            mesh.SetMeshHeights(meshResolution, meshResolution, heightMap, heightRange, 
                computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetMeshHeights(this Mesh mesh, 
            int meshResolutionX, int meshResolutionY, Texture2D heightMap, 
            Vector2 heightRange,
            bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) { 
            int vertexCount = (meshResolutionX + 1) * (meshResolutionY + 1);
            if (mesh.vertexCount != vertexCount)
                throw new Exception("Mesh resolution doesn't match mesh topology.");
            Color[] heightColors = heightMap.SampleTexture(meshResolutionX, meshResolutionY);
            float[] heights = new float[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                heights[i] = Mathf.Lerp(heightRange.x, heightRange.y, heightColors[i].grayscale);
            mesh.SetMeshHeights(heights, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetMeshHeights(this Mesh mesh, float[] heights,
            bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            if (vertices.Count == 0) 
                throw new Exception("Mesh topology is invalid.");
            if (vertices.Count != heights.Length)
                throw new Exception("Mesh and heights must have the same number of vertices.");

            for (int i = 0; i < vertices.Count; i++) {
                vertices[i] = vertices[i].XZ().ToXZ(heights[i]);
            }
            mesh.SetVertices(vertices);
            if (computeNormals) mesh.RecalculateNormals();
            if (computeBounds) mesh.RecalculateBounds();
            mesh.MarkModified();
            mesh.UploadMeshData(isMeshFinal);
        }
	}
}