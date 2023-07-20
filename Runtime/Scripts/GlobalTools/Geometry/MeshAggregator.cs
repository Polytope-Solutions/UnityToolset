using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
	public static class MeshAggregator {
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

	}
}