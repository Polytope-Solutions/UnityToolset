using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
	public static class MeshAggregator {
		public static Mesh JoinMeshes(Mesh[] sources) {
			Mesh mesh = new Mesh();

			List<Vector3> vertices = new List<Vector3>();
			List<int> indices = new List<int>();

			int currentOffset = 0;
			foreach (Mesh item in sources) { 
				vertices.AddRange(item.vertices);
				item.triangles.ToList().ForEach(index => indices.Add(index + currentOffset));
				currentOffset += item.vertices.Length;
			}

			if (vertices.Count >= 65536)
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			mesh.vertices = vertices.ToArray();
			mesh.triangles = indices.ToArray();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}

	}
}