using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public static partial class MeshTools {
        public static (MeshData meshData, Mesh mesh) ExtrudeCurve(List<Vector3> points,
                float height, Vector3 upDirection, bool capTop,
                List<List<Vector3>> innerCurves = null,
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            Mesh mesh = new Mesh();
            MeshData meshData = ExtrudeCurve(points, height, upDirection, capTop, innerCurves);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
            return (meshData, mesh);
        }
        public static bool EvaluateCurveWindingInPlane(List<Vector3> curve, Vector3 normal) {
            Matrix4x4 correction = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(normal, Vector3.up), Vector3.one);
            float winding = 0;
            for (int i = 0; i < curve.Count; i++) {
                Vector3 planarVertex = correction * curve[i], planarNextVertex = curve[(i + 1) % curve.Count];
                winding += (planarVertex.x + planarNextVertex.x) * (planarVertex.z - planarNextVertex.z);
            }
            return winding > 0;
        }
        // NB! expexts curves in order without duplicate point on the end to close loop (repeats start point for better UV mapping)
        // Applies uv in equal distance per side over v (in bottom half if capping top) with a gap between sides
        // Applies normals out from centers or in to centers for internal curves.
        // TODO: add unweld sides option to have more realistic normals and lighting.
        public static MeshData ExtrudeCurve(List<Vector3> outerCurve,
                float height, Vector3 upDirection,
                bool capTop,
                List<List<Vector3>> innerCurves = null) {
            bool hasInnerCurves = (innerCurves != null && innerCurves.Count > 0);
            List<Vector3> topCurve = new List<Vector3>();
            List<List<Vector3>> topInnerCurves = new List<List<Vector3>>();
            List<Vector3> innerCurveCenters = new List<Vector3>();

            // Compute outer curve center for normals
            Vector3 outerCurveCenter = Vector3.zero;
            foreach (Vector3 vertex in outerCurve)
                outerCurveCenter += vertex;
            outerCurveCenter /= outerCurve.Count;

            int vertexCount = (outerCurve.Count + 1) * 2;
            int triangleCount = (outerCurve.Count + 1) * 2;
            if (hasInnerCurves)
                foreach (List<Vector3> innerCurve in innerCurves) {
                    vertexCount += (innerCurve.Count + 1) * 2;
                    triangleCount += (innerCurve.Count + 1) * 2;
                    // Compute center for normals
                    Vector3 innerCurveCenter = Vector3.zero;
                    foreach (Vector3 vertex in innerCurve)
                        innerCurveCenter += vertex;
                    innerCurveCenter /= innerCurve.Count;
                    innerCurveCenters.Add(innerCurveCenter);
                }
            MeshData meshData = new MeshData(vertexCount, triangleCount * 3, true, true);


            // Add outer curve vertices, accounting for clockwise or counter-clockwise winding.
            {
                bool clockwise = EvaluateCurveWindingInPlane(outerCurve, upDirection);
                for (int j = 0; j < outerCurve.Count + 1; j++) {
                    Vector3 vertex = outerCurve[(clockwise ? j : 2 * outerCurve.Count - 1 - j) % outerCurve.Count],
                        topVertex = vertex + upDirection * height,
                        normal = (vertex - outerCurveCenter).normalized;
                    meshData.SetVertex(j, vertex,
                        new Vector2(j / ((float)vertexCount / 2), 0),
                        normal);
                    meshData.SetVertex(j + outerCurve.Count + 1, topVertex,
                        new Vector2(j / ((float)vertexCount / 2), (!capTop) ? 1 : 0.5f),
                        normal);
                    if (j < outerCurve.Count)
                        topCurve.Add(topVertex);
                }
            }
            // Add inner curve vertices, accounting for clockwise or counter-clockwise winding.
            if (hasInnerCurves) {
                int startIndex = (outerCurve.Count + 1) * 2;
                for (int i = 0; i < innerCurves.Count; i++) {
                    List<Vector3> innerCurve = innerCurves[i],
                        topInnerCurve = new List<Vector3>();
                    bool clockwise = EvaluateCurveWindingInPlane(innerCurve, upDirection);
                    for (int j = 0; j < innerCurve.Count + 1; j++) {
                        Vector3 vertex = innerCurve[(clockwise ? j : (2 * innerCurve.Count - 1 - j)) % innerCurve.Count],
                            topVertex = vertex + upDirection * height,
                            normal = -(vertex - innerCurveCenters[i]).normalized;
                        meshData.SetVertex(startIndex + j, vertex,
                            new Vector2((startIndex / 2 + j) / ((float)vertexCount / 2), 0),
                            normal);
                        meshData.SetVertex(startIndex + j + innerCurve.Count + 1, topVertex,
                            new Vector2((startIndex / 2 + j) / ((float)vertexCount / 2), (!capTop) ? 1 : 0.5f),
                            normal);
                        if (j < innerCurve.Count)
                            topInnerCurve.Add(topVertex);
                    }
                    topInnerCurves.Add(topInnerCurve);
                    startIndex += (innerCurve.Count + 1) * 2;
                }
            }

            // Add triangles for outer walls
            for (int i = 0; i < outerCurve.Count; i++) {
                int ti = i * 6;
                int a = i, b = i + 1,
                    c = i + outerCurve.Count + 1, d = i + 1 + outerCurve.Count + 1;
                meshData.SetTriangle(ti,
                    a, b, c);
                meshData.SetTriangle(ti + 3,
                    b, d, c);
            }
            // Add triangles for inner walls (in counterClockwise manner as indices are in clockwise but triangles shoud face inwards).
            if (hasInnerCurves) {
                int startIndex = (outerCurve.Count + 1) * 2, triangleStartIndex = outerCurve.Count * 6;
                foreach (List<Vector3> innerCurve in innerCurves) {
                    for (int i = 0; i < innerCurve.Count; i++) {
                        int ti = i * 6 + triangleStartIndex;
                        int a = i + startIndex, b = i + 1 + startIndex,
                            c = i + innerCurve.Count + 1 + startIndex, d = i + 1 + innerCurve.Count + 1 + startIndex;
                        meshData.SetTriangle(ti,
                            a, c, b);
                        meshData.SetTriangle(ti + 3,
                            b, c, d);
                    }
                    startIndex += (innerCurve.Count + 1) * 2;
                    triangleStartIndex += innerCurve.Count * 6;
                }
            }

            if (capTop && outerCurve.Count > 2) {
                MeshData topMeshData;
                //topMeshData = CapTopSimple(topCurve, upDirection);
                topMeshData = CapTop(topCurve, upDirection, topInnerCurves);
                meshData.Merge(topMeshData);
            }
            return meshData;
        }
        public static MeshData CapTopSimple(List<Vector3> outerCurve,
                Vector3 upDirection) {
            MeshData meshData = new MeshData(outerCurve.Count, (outerCurve.Count - 2) * 3, true, true);
            //List<int> indices = Enumerable.Range(2, points.Count).ToList();

            for (int i = 0; i < outerCurve.Count; i++) {
                Vector3 vertex = outerCurve[i];
                // TODO: should have other UVs
                meshData.SetVertex(i, vertex,
                    new Vector2(i / (float)(outerCurve.Count - 1), 1), upDirection);
            }

            int startIndex = 0;
            int a, b, c, d;
            float dotC, dotD;
            Vector3 currentSide, sideC, sideD, inplaneCurrentSideNormal;
            a = 0;
            b = 1;
            do {
                // Find next and previous points.
                c = b + 1;
                d = (a - 1 + outerCurve.Count) % outerCurve.Count;
                // Find vectors for the current and two possible next sides.
                currentSide = outerCurve[b] - outerCurve[a];
                sideC = outerCurve[c] - outerCurve[b];
                sideD = outerCurve[d] - outerCurve[a];
                // Compute in plane normal to check if on the correct side.
                inplaneCurrentSideNormal = //(clockwise) ?
                    Vector3.Cross(upDirection, currentSide).normalized;
                //: Vector3.Cross(currentSide, upDirection).normalized;
                // Compute how alligned are the two next possible sides with the normal.
                dotC = Vector3.Dot(inplaneCurrentSideNormal, sideC);
                dotD = Vector3.Dot(inplaneCurrentSideNormal, sideD);
                // Pick a side and change indices for the next iteration.
                if (dotC > dotD) {
                    meshData.SetTriangle(startIndex,
                        a + outerCurve.Count, b + outerCurve.Count, c + outerCurve.Count);
                    //((clockwise) ? b : c) + outerCurve.Count, ((clockwise) ? c : b) + outerCurve.Count);
                    b = c;
                    startIndex += 3;
                }
                else {
                    meshData.SetTriangle(startIndex,
                        a + outerCurve.Count, b + outerCurve.Count, d + outerCurve.Count);
                    //((clockwise) ? b : d) + outerCurve.Count, ((clockwise) ? d : b) + outerCurve.Count);
                    a = d;
                    startIndex += 3;
                }
                // Repeat until after both c and d are the same (finished processing all points).
            } while (c != d);
            return meshData;
        }

        // NB! Expects curves organized in clockwise order to face correctly in the up direction.
        // Applies flat prorjection to plane defined by normal in top half of UV.
        // Applies normal along up direction.
        public static MeshData CapTop(List<Vector3> outerCurve,
                Vector3 upDirection,
                List<List<Vector3>> innerCurves = null) {
            List<Vector3> totalPoints = new List<Vector3>();
            Matrix4x4 reorient = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(upDirection, Vector3.up), Vector3.one);
            Vector2 minMaxU = new Vector2(float.MaxValue, float.MinValue),
                minMaxV = new Vector2(float.MaxValue, float.MinValue);
            outerCurve.ForEach(vertex => {
                totalPoints.Add(vertex);
                Vector3 reorientedPoint = reorient.MultiplyPoint3x4(vertex);
                minMaxU.x = Mathf.Min(minMaxU.x, reorientedPoint.x);
                minMaxU.y = Mathf.Max(minMaxU.y, reorientedPoint.x);
                minMaxV.x = Mathf.Min(minMaxV.x, reorientedPoint.z);
                minMaxV.y = Mathf.Max(minMaxV.y, reorientedPoint.z);
            });
            List<int> triangles = new List<int>();

            List<Vector3> innerCurveCenters = new List<Vector3>();
            List<List<int>> innerIndices = new List<List<int>>();
            Vector3 temp;
            int start = outerCurve.Count;
            if (innerCurves != null)
                foreach (List<Vector3> innerCurve in innerCurves) {
                    innerCurve.ForEach(item => totalPoints.Add(item));

                    temp = Vector3.zero;
                    innerCurve.ForEach(item => temp += item);
                    temp /= innerCurve.Count;
                    innerCurveCenters.Add(temp);
                    innerIndices.Add(Enumerable.Range(start, innerCurve.Count).ToList());
                    start += innerCurve.Count;
                }

            List<List<int>> availableOuterCurves = new List<List<int>>();
            List<int> currentOuterCurveIndices = Enumerable.Range(0, outerCurve.Count).ToList();
            availableOuterCurves.Add(currentOuterCurveIndices);
            while (availableOuterCurves.Count > 0) {
                currentOuterCurveIndices = availableOuterCurves[0];
                // select two points
                int a = 0, b = 1;
                Vector3 side = totalPoints[currentOuterCurveIndices[b]] - totalPoints[currentOuterCurveIndices[a]];
                Vector3 sideCenter = (totalPoints[currentOuterCurveIndices[b]] + totalPoints[currentOuterCurveIndices[a]]) / 2;
                Vector3 sideInPlaneNormal = Vector3.Cross(upDirection, side);

                bool found = false;
                int selectedInnerCurve = -1;
                int selectedInnerPointIndex = -1;
                if (innerIndices.Count > 0) {
                    // loop through inner curves
                    selectedInnerCurve = 0;
                    float allignmentTemp;
                    float minDistance = float.MaxValue, currentDistance;
                    for (int i = 0; i < innerIndices.Count; i++) {
                        for (int j = 0; j < innerIndices[selectedInnerCurve].Count; j++) {
                            currentDistance = (totalPoints[innerIndices[selectedInnerCurve][j]]
                                - sideCenter)
                                .sqrMagnitude;
                            allignmentTemp = Vector3.Dot(totalPoints[innerIndices[selectedInnerCurve][j]]
                                - sideCenter, sideInPlaneNormal);
                            if (allignmentTemp > 0 && currentDistance < minDistance) {
                                selectedInnerCurve = i;
                                minDistance = currentDistance;
                                selectedInnerPointIndex = innerIndices[selectedInnerCurve][j];
                            }
                        }
                    }
                    found = selectedInnerPointIndex >= 0;
                }

                // select next and previous
                int c = a, d = b;
                Vector3 nextSide = Vector3.zero, previousSide = Vector3.zero;
                float allignmentNext = float.MinValue, allignmentPrevious = float.MinValue;
                List<int> previousLoopIndices = new List<int>();
                List<int> nextLoopIndices = new List<int>();
                previousLoopIndices.Add(currentOuterCurveIndices[a]);
                nextLoopIndices.Add(currentOuterCurveIndices[b]);
                Vector3 nextNextSide, previousNextSide;
                do {
                    if (c == a || allignmentPrevious < 0) {
                        c = (c - 1 + currentOuterCurveIndices.Count) % currentOuterCurveIndices.Count;
                        previousLoopIndices.Add(currentOuterCurveIndices[c]);
                        previousSide = totalPoints[currentOuterCurveIndices[a]] - totalPoints[currentOuterCurveIndices[c]];
                        allignmentPrevious = -Vector3.Dot(previousSide, sideInPlaneNormal);
                    }
                    if (d == b || allignmentNext < 0) {
                        d = d + 1;
                        nextLoopIndices.Add(currentOuterCurveIndices[d]);
                        nextSide = totalPoints[currentOuterCurveIndices[d]] - totalPoints[currentOuterCurveIndices[b]];
                        allignmentNext = Vector3.Dot(nextSide, sideInPlaneNormal);
                    }
                    // are they on the correct side?
                    // - no - select next ansd repeat
                    // - yes - continue
                } while ((allignmentNext < 0 || allignmentPrevious < 0) && (c > d));
                nextNextSide = totalPoints[currentOuterCurveIndices[d]] - totalPoints[currentOuterCurveIndices[a]];
                previousNextSide = totalPoints[currentOuterCurveIndices[c]] - totalPoints[currentOuterCurveIndices[b]];
                Vector3 nextNextNormalInPlane = Vector3.Cross(nextNextSide, upDirection),
                    previousNextNormalInPlane = Vector3.Cross(upDirection, previousNextSide);

                // Check if inner point was selected, is it inside one of the next triangles
                if (found) {
                    Vector3 innerPointA = totalPoints[selectedInnerPointIndex] - totalPoints[currentOuterCurveIndices[a]],
                        innerPointB = totalPoints[selectedInnerPointIndex] - totalPoints[currentOuterCurveIndices[b]];
                    Vector3 nextNormal = Vector3.Cross(upDirection, nextSide);
                    bool insideNext =
                        Vector3.Dot(nextNormal, innerPointB) > 0
                        && Vector3.Dot(nextNextNormalInPlane, innerPointA) > 0;
                    found = insideNext;
                    if (!found) {
                        Vector3 previousNormal = Vector3.Cross(upDirection, previousSide);
                        bool insidePrevious = Vector3.Dot(previousNormal, innerPointA) > 0
                            && Vector3.Dot(previousNextNormalInPlane, innerPointB) > 0;
                        found = insidePrevious;
                    }
                }

                // if adding merging inner point
                if (found) {
                    triangles.Add(currentOuterCurveIndices[a]);
                    triangles.Add(currentOuterCurveIndices[b]);
                    triangles.Add(selectedInnerPointIndex);

                    // reorganize the curves:
                    List<int> newPoints = new List<int>();
                    int curveStartIndex = innerIndices[selectedInnerCurve][0];
                    for (int i = 0; i < innerIndices[selectedInnerCurve].Count; i++) {
                        int j =
                            curveStartIndex
                            + (selectedInnerPointIndex - curveStartIndex + i)
                                % innerIndices[selectedInnerCurve].Count;
                        newPoints.Add(j);
                    }
                    newPoints.Add(selectedInnerPointIndex);
                    newPoints.Reverse();
                    currentOuterCurveIndices.InsertRange(1, newPoints);

                    innerIndices.RemoveAt(selectedInnerCurve);
                    innerCurveCenters.RemoveAt(selectedInnerCurve);
                }
                else {
                    bool nextInPrevious = c != d && Vector3.Dot(previousNextNormalInPlane, nextSide) >= 0;
                    float distanceNext = (totalPoints[currentOuterCurveIndices[d]] - sideCenter).sqrMagnitude,
                        distancePrevious = (totalPoints[currentOuterCurveIndices[c]] - sideCenter).sqrMagnitude;
                    if ((nextInPrevious && allignmentNext > 0) || (allignmentNext > 0 && distanceNext < distancePrevious)) {
                        triangles.Add(currentOuterCurveIndices[a]);
                        triangles.Add(currentOuterCurveIndices[b]);
                        triangles.Add(currentOuterCurveIndices[d]);
                        if (nextLoopIndices.Count > 2) {
                            for (int i = 0; i < nextLoopIndices.Count - 1; i++) {
                                currentOuterCurveIndices.RemoveAt(b);
                            }
                            availableOuterCurves.Add(nextLoopIndices);
                        }
                        else
                            currentOuterCurveIndices.RemoveAt(b);
                    }
                    else {
                        triangles.Add(currentOuterCurveIndices[c]);
                        triangles.Add(currentOuterCurveIndices[a]);
                        triangles.Add(currentOuterCurveIndices[b]);
                        if (previousLoopIndices.Count > 2) {
                            int index = a;
                            for (int i = 0; i < previousLoopIndices.Count - 1; i++) {
                                currentOuterCurveIndices.RemoveAt(index);
                                index = (index - 1 + currentOuterCurveIndices.Count) % currentOuterCurveIndices.Count;
                            }
                            previousLoopIndices.Reverse();
                            availableOuterCurves.Add(previousLoopIndices);
                        }
                        else
                            currentOuterCurveIndices.RemoveAt(a);
                    }
                }
                if (currentOuterCurveIndices.Count == 2)
                    availableOuterCurves.RemoveAt(0);
            }
            MeshData meshData = new MeshData(totalPoints.Count, triangles.Count, true, true);

            for (int i = 0; i < totalPoints.Count; i++) {
                Vector3 reorientedPoint = reorient.MultiplyPoint3x4(totalPoints[i]);
                Vector2 uv = new Vector2(
                    Mathf.InverseLerp(minMaxU.x, minMaxU.y, reorientedPoint.x),
                    Mathf.Lerp(0.5f, 1f, Mathf.InverseLerp(minMaxV.x, minMaxV.y, reorientedPoint.z))
                );
                meshData.SetVertex(i, totalPoints[i],
                    uv, upDirection);
            }
            for (int i = 0; i < triangles.Count / 3; i++) {
                meshData.SetTriangle(i * 3,
                    triangles[i * 3], triangles[i * 3 + 1], triangles[i * 3 + 2]
                );
            }
            return meshData;
        }
    }
}