using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Globalization;
using System.Linq;

namespace PolytopeSolutions.Toolset.Files {
    public class OBJHandler {
        public static void Encode(string name, Transform[] transforms, Mesh[] meshes,
                out string meshData, out string materialData,
                string[] names = null, Material[] materials = null,
                bool saveUVs=true, bool saveNormals=true, bool embedTextureDetails=false) {
            StringBuilder meshDataBuilder = new StringBuilder(), materialDataBuilder = new StringBuilder();
            bool useNames = ((names != null) && (names.Length == meshes.Length)),
                useMaterials = ((materials != null) && (materials.Length == meshes.Length));
            string objFileName = name + ".obj", mtlFileName = name + ".mtl";
            string numberPrecisionFormat = "F4";

            // Header.
            meshDataBuilder.AppendLine("# PolytopeSolutions");
            meshDataBuilder.AppendLine(); meshDataBuilder.AppendLine();
            // Reference to the material file.
            if (useMaterials) {
                meshDataBuilder.AppendFormat("mtllib {0}", mtlFileName);
                meshDataBuilder.AppendLine();
            }

            string objectName;
            int channelCount = 0;
            int lastVertexCount = 0, currentVertexCount = 0;
            int lastUVCount = 0, currentUVCount = 0;
            int lastNormalCount = 0, currentNormalCount = 0;
            for (int i = 0; i < meshes.Length; i++) {
                meshDataBuilder.AppendLine();
                meshDataBuilder.AppendLine("# MESH-"+i);
                // Deliniate an object.
                objectName = (useNames) ? names[i] : meshes[i].name;
                meshDataBuilder.AppendFormat("g {0}", objectName.Replace(" ", "_"));
                meshDataBuilder.AppendLine();
                meshDataBuilder.AppendFormat("o {0}", objectName.Replace(" ", "_"));
                meshDataBuilder.AppendLine();
                // Reference a material.
                if (useMaterials) {
                    meshDataBuilder.AppendFormat("usemtl {0}", materials[i].name.Replace(" ", "_"));
                    meshDataBuilder.AppendLine();
                }
                // Save vertices.
                List<Vector3> vertices = new List<Vector3>();
                meshes[i].GetVertices(vertices);
                meshDataBuilder.AppendLine("# Vertices.");
                for (int j = 0; j < vertices.Count; j++, currentVertexCount++) {
                    vertices[j] = transforms[i].TransformPoint(vertices[j]);
                    meshDataBuilder.AppendFormat("v {0} {1} {2}",
                        (-vertices[j].x).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), (vertices[j].y).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), (vertices[j].z).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                    meshDataBuilder.AppendLine();
                }
                // Save UVs.
                if (saveUVs) {
                    channelCount = 0;
                    meshDataBuilder.AppendLine("# UVs.");
                    bool foundAny = false;
                    int c = 0; {
                    //for (int c = 0; c < 8; c++) {
                        List<Vector2> uvs = new List<Vector2>();
                        meshes[i].GetUVs(c, uvs);
                        if (uvs.Count > 0) {
                            meshDataBuilder.AppendLine("#\tChannel " + c);
                            channelCount++;
                        }
                        for (int j = 0; j < uvs.Count; j++, currentUVCount++) {
                            meshDataBuilder.AppendFormat("vt {0} {1}",
                                uvs[j].x.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), uvs[j].y.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                            meshDataBuilder.AppendLine();
                        }
                        foundAny |= (uvs.Count > 0);
                    }
                    saveUVs &= foundAny;
                }
                // Save normals.
                if (saveNormals) {
                    List<Vector3> normals = new List<Vector3>();
                    meshes[i].GetNormals(normals);
                    if (normals.Count > 0)
                        meshDataBuilder.AppendLine("# Normals.");
                    for (int j = 0; j < normals.Count; j++, currentNormalCount++) {
                        normals[j] = transforms[i].TransformDirection(normals[j]);
                        meshDataBuilder.AppendFormat("vn {0} {1} {2}",
                            (normals[j].x).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), (normals[j].y).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), (normals[j].z).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                        meshDataBuilder.AppendLine();
                    }
                    saveNormals &= (normals.Count > 0);
                }
                // Save Triangles.
                meshDataBuilder.AppendLine("# Faces.");
                for (int sm = 0; sm < meshes[i].subMeshCount; sm++) {
                    List<int> indices = new List<int>();
                    meshes[i].GetTriangles(indices, sm);
                    if (meshes[i].subMeshCount > 1)
                        meshDataBuilder.AppendLine("#\tSubmesh " + sm);
                    for (int j = 0; j < indices.Count; j+=3) {
                        meshDataBuilder.Append("f");

                        // Vertex 1
                        meshDataBuilder.AppendFormat(" {0}", lastVertexCount + indices[j] + 1);
                        if (saveUVs)            meshDataBuilder.AppendFormat("/{0}", lastUVCount + indices[j] + 1);
                        else if (saveNormals)   meshDataBuilder.Append("/");
                        if (saveNormals)        meshDataBuilder.AppendFormat("/{0}", lastNormalCount + indices[j] + 1);
                        // Vertex 2
                        meshDataBuilder.AppendFormat(" {0}", lastVertexCount + indices[j+2] + 1);
                        if (saveUVs) meshDataBuilder.AppendFormat("/{0}", lastUVCount + indices[j+2] + 1);
                        else if (saveNormals) meshDataBuilder.Append("/");
                        if (saveNormals) meshDataBuilder.AppendFormat("/{0}", lastNormalCount + indices[j+2] + 1);
                        // Vertex 3
                        meshDataBuilder.AppendFormat(" {0}", lastVertexCount + indices[j+1] + 1);
                        if (saveUVs) meshDataBuilder.AppendFormat("/{0}", lastUVCount + indices[j+1] + 1);
                        else if (saveNormals) meshDataBuilder.Append("/");
                        if (saveNormals) meshDataBuilder.AppendFormat("/{0}", lastNormalCount + indices[j+1] + 1);

                        meshDataBuilder.AppendLine();
                    }
                }
                lastVertexCount = currentVertexCount;
                lastUVCount = currentUVCount;
                lastNormalCount = currentNormalCount;
            }

            meshData = meshDataBuilder.ToString();

            if (useMaterials) {
                // Header.
                materialDataBuilder.AppendLine("# PolytopeSolutions");

                for (int i = 0; i < materials.Length; i++) {
                    materialDataBuilder.AppendLine(); materialDataBuilder.AppendLine();
                    materialDataBuilder.AppendLine("# MATERIAL-" + i);
                    // Deliniate a material.
                    materialDataBuilder.AppendFormat("newmtl {0}", materials[i].name.Replace(" ", "_"));
                    materialDataBuilder.AppendLine();
                    //// - ambient
                    //materialDataBuilder.AppendFormat("\tKa {0} {1} {2}",
                    //    materials[i].color.r.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].color.g.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].color.b.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                    //materialDataBuilder.AppendLine();
                    // - diffuse
                    materialDataBuilder.AppendFormat("\tKd {0} {1} {2}",
                        materials[i].color.r.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].color.g.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].color.b.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                    materialDataBuilder.AppendLine();
                    // - transparency
                    materialDataBuilder.AppendFormat("\td {0}",
                        materials[i].color.a.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                    materialDataBuilder.AppendLine();
                    // - texture
                    if (materials[i].mainTexture != null) {
                        materialDataBuilder.Append("\tmap_Kd ");
                        if (embedTextureDetails) {
                            materialDataBuilder.AppendFormat("-o {0} {1} {2} -s {3} {4} {5} ",
                                materials[i].mainTextureOffset.x.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].mainTextureOffset.y.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), (0.0).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat),
                                materials[i].mainTextureScale.x.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].mainTextureScale.y.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), (1.0).ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));

                        }
                        materialDataBuilder.AppendFormat("{0}",
                            materials[i].mainTexture.name + ".png");

                    materialDataBuilder.AppendLine();
                    }
                    //// - specular
                    //materialDataBuilder.AppendFormat("\tKs {0} {1} {2}",
                    //    materials[i].color.r.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].color.g.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), materials[i].color.b.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                    //materialDataBuilder.AppendLine();
                    //// - specular exponent
                    //materialDataBuilder.AppendFormat("\tNs {0}", 0f.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                    //materialDataBuilder.AppendLine();
                    //// - transmission filter
                    //materialDataBuilder.AppendFormat("\tTf {0} {1} {2}",
                    //    Color.black.r.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), Color.black.g.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat), Color.black.b.ToString(numberPrecisionFormat, CultureInfo.InvariantCulture.NumberFormat));
                    //materialDataBuilder.AppendLine();

                }
            }
            materialData = materialDataBuilder.ToString();
        }
    }
}