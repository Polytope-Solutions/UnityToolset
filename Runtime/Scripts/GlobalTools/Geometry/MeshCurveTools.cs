using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public static partial class MeshTools {
        private static readonly float Epsilon = 0.000001f;
        public static (MeshData meshData, Mesh mesh) ExtrudeCurve(List<Vector3> points,
                float startHeight, float endHeight, Vector3 upDirection,
                List<List<Vector3>> innerCurves = null,
                bool makeClosed = true, bool capTop = true, bool faceIn = false, bool splitFaces = true,
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            Mesh mesh = new Mesh();
            MeshData meshData = ExtrudeCurve(points, startHeight, endHeight, upDirection, innerCurves,
                makeClosed, capTop, faceIn, splitFaces);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
            return (meshData, mesh);
        }
        public static bool EvaluateCurveWindingInPlane(List<Vector3> curve, Vector3 normal) {
            Matrix4x4 correction = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(normal, Vector3.up), Vector3.one);
            float winding = 0;
            Vector3 planarVertex, planarVertexNext = correction * curve[curve.Count - 1];
            for (int i = 0; i < curve.Count; i++) {
                planarVertex = planarVertexNext;
                planarVertexNext = correction * curve[i];
                winding += (planarVertexNext.x - planarVertex.x) * (planarVertexNext.z + planarVertex.z);
            }
            return winding > 0;
        }
        // Forces curve to be in clockwise order if closed. Positive is outwards.
        public static void OffsetCurve(List<Vector3> curve, Vector3 upDirection, float offset, ref List<Vector3> offsetCurve,
                bool isClosed) {
            offsetCurve.Clear();
            if (isClosed) {
                // Make sure first and last are not duplicates
                if ((curve[curve.Count - 1] - curve[0]).sqrMagnitude < Epsilon) {
                    curve.RemoveAt(curve.Count - 1);
                }
                bool clockwise = EvaluateCurveWindingInPlane(curve, upDirection);
                if (!clockwise)
                    curve.Reverse();
            }
            int currentIndex = (isClosed) ? 0 : 1, previousIndex = (isClosed) ? curve.Count - 1 : 0;
            Vector3 currentVertex = curve[currentIndex], previousVertex = curve[previousIndex],
                currentSide = currentVertex - previousVertex, previousSide,
                currentInPlaneNormal = Vector3.Cross(currentSide, upDirection).normalized, previousInPlaneNormal,
                shift;
            float angle, magnitude;
            // Add the start if not closed.
            if (!isClosed)
                offsetCurve.Add(curve[0] + currentInPlaneNormal * offset);
            // go through sides, evalueate angles with porevious, compute offset vector and add points
            for (int i = (isClosed) ? 1 : 2; i < curve.Count + ((isClosed) ? 1 : 0); i++) {
                previousIndex = currentIndex;
                currentIndex = i % curve.Count;

                previousVertex = currentVertex;
                currentVertex = curve[currentIndex];

                previousSide = currentSide;
                currentSide = currentVertex - curve[previousIndex];

                previousInPlaneNormal = currentInPlaneNormal;
                currentInPlaneNormal = Vector3.Cross(currentSide, upDirection).normalized;

                angle = Mathf.Abs(Vector3.SignedAngle(-previousSide, currentSide, upDirection));
                magnitude = offset / Mathf.Sin(angle * Mathf.Deg2Rad / 2);
                shift = (previousInPlaneNormal + currentInPlaneNormal).normalized * magnitude;
                offsetCurve.Add(previousVertex + shift);
            }
            // Add the end if not closed.
            if (!isClosed)
                offsetCurve.Add(currentVertex + currentInPlaneNormal * offset);
        }
        // Applies uv in equal distance per side over v (in bottom half if capping top) with a gap between sides
        // Applies normals out from centers or in to centers for internal curves.
        public static MeshData ExtrudeCurve(List<Vector3> outerCurve,
                float startHeight, float endHeight, Vector3 upDirection,
                List<List<Vector3>> innerCurves = null,
                bool makeClosed = true, bool capTop = true, bool faceIn = false, bool splitFaces = true) {
            MeshData meshData = new MeshData();
            meshData.Merge(
                ExtrudeCurveBase(outerCurve, startHeight, endHeight, upDirection, makeClosed, capTop, faceIn, splitFaces)
            );
            bool hasInnerCurves = (innerCurves != null && innerCurves.Count > 0);
            if (hasInnerCurves)
                foreach (List<Vector3> innterCurve in innerCurves) {
                    meshData.Merge(
                        ExtrudeCurveBase(innterCurve, startHeight, endHeight, upDirection, true, capTop, !faceIn, splitFaces)
                    );
                }
            if (capTop) {
                meshData.Merge(
                    TriangulateCurve(outerCurve, upDirection, endHeight, innerCurves)
                );
            }

            return meshData;
        }
        private static MeshData ExtrudeCurveBase(List<Vector3> curve,
                float startHeight, float endHeight, Vector3 upDirection,
                bool makeClosed = true, bool capTop = true, bool faceIn = false, bool splitFaces = true) {
            if (makeClosed) {
                if (splitFaces) {
                    if ((curve[curve.Count - 1] - curve[0]).sqrMagnitude < Epsilon) {
                        // last matches first - remove as split automatically
                        curve.RemoveAt(curve.Count - 1);
                    }
                }
                else {
                    if ((curve[curve.Count - 1] - curve[0]).sqrMagnitude > Epsilon) {
                        // last doesn't matches first - add first to end to simplify triangulation
                        curve.Add(curve[0]);
                    }
                }
            }

            int vertexCount = (splitFaces) ? curve.Count * 4 : curve.Count * 2;
            int uvXCount = (makeClosed) ? curve.Count + 1 : curve.Count;
            int faceCount = (makeClosed) ? curve.Count : curve.Count - 1;
            int triangleCount = faceCount * 2;
            MeshData meshData = new MeshData(vertexCount, triangleCount * 3, true, true);

            // Add curve vertices, accounting for clockwise or counter-clockwise winding.
            Vector3 baseVertex, nextBaseVertex, bottomVertex, topVertex, normal, prevNormal;
            int uvX;
            bool clockwise = EvaluateCurveWindingInPlane(curve, upDirection);
            if (clockwise == faceIn) {
                curve.Reverse();
                clockwise = !clockwise;
            }

            // Compute outer curve center for normals
            Vector3 curveCenter = Vector3.zero;
            if (!splitFaces) {
                foreach (Vector3 temp in curve)
                    curveCenter += temp;
                curveCenter /= curve.Count;
            }

            baseVertex = curve[curve.Count - 1];//[clockwise ? curve.Count - 1 : 1];
            nextBaseVertex = curve[0];
            normal = ((splitFaces) ?
                Vector3.Cross(nextBaseVertex - baseVertex, upDirection)
                : baseVertex - curveCenter
            ).normalized;
            for (int j = 0; j < curve.Count; j++) {
                baseVertex = nextBaseVertex;
                nextBaseVertex = curve[(j + 1) % curve.Count];//[(clockwise ? j + 1 : 2 * curve.Count - 1 - j - 1) % curve.Count];
                bottomVertex = baseVertex + upDirection * startHeight;
                topVertex = baseVertex + upDirection * endHeight;
                prevNormal = normal;
                normal = ((splitFaces) ?
                    Vector3.Cross(nextBaseVertex - baseVertex, upDirection)
                    : baseVertex - curveCenter
                ).normalized;
                meshData.SetVertex(j, bottomVertex,
                    new Vector2(j / (float)uvXCount, 0),
                    normal);
                meshData.SetVertex(j + curve.Count, topVertex,
                    new Vector2(j / (float)uvXCount, (!capTop) ? 1 : 0.5f),
                    normal);
                if (splitFaces) {
                    uvX = (j == 0) ? curve.Count : j;
                    meshData.SetVertex(j + curve.Count * 2, bottomVertex,
                        new Vector2(uvX / (float)uvXCount, 0),
                        prevNormal);
                    meshData.SetVertex(j + curve.Count * 3, topVertex,
                        new Vector2(uvX / (float)uvXCount, (!capTop) ? 1 : 0.5f),
                        prevNormal);
                }
            }

            // Add triangles for outer walls
            for (int i = 0; i < faceCount; i++) {
                int ti = i * 6;
                int a = i, b = (i + 1) % curve.Count,
                    c = a + curve.Count, d = b + curve.Count;
                if (splitFaces) {
                    b += curve.Count * 2;
                    d += curve.Count * 2;
                }
                meshData.SetTriangle(ti,
                    a, b, c);
                meshData.SetTriangle(ti + 3,
                    b, d, c);
            }
            return meshData;
        }

        // Applies flat prorjection to plane defined by normal in top half of UV.
        // Applies normal along up direction.
        public static MeshData TriangulateCurve(List<Vector3> outerCurve, Vector3 upDirection, float upOffset,
            List<List<Vector3>> innerCurves = null) {

            bool clockwise = EvaluateCurveWindingInPlane(outerCurve, upDirection);
            if (!clockwise)
                outerCurve.Reverse();

            List<Vector3> totalPoints = new();
            List<int> totalIndices = new();
            List<int> totalTriangles = new();
            int i, j, k;
            Matrix4x4 reorient = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(upDirection, Vector3.up), Vector3.one);
            Vector2 minMaxU = new Vector2(float.MaxValue, float.MinValue),
                minMaxV = new Vector2(float.MaxValue, float.MinValue);
            Vector3 temp, prevPoint;

            // Register outer curve - reducing any duplicates.
            prevPoint = outerCurve[outerCurve.Count - 1];
            for (j = 0; j < outerCurve.Count; j++) {
                // skip duplicates
                if ((outerCurve[j] - prevPoint).sqrMagnitude < Epsilon)
                    continue;
                prevPoint = outerCurve[j];
                totalIndices.Add(totalPoints.Count);
                totalPoints.Add(outerCurve[j] + upDirection * upOffset);
                temp = reorient.MultiplyPoint3x4(outerCurve[j]);
                minMaxU.x = Mathf.Min(minMaxU.x, temp.x);
                minMaxU.y = Mathf.Max(minMaxU.y, temp.x);
                minMaxV.x = Mathf.Min(minMaxV.x, temp.z);
                minMaxV.y = Mathf.Max(minMaxV.y, temp.z);
            }

            // Merge the inner cuves into the outercurve bridging to the closest point from inner to outer recursively
            if (innerCurves != null && innerCurves.Count > 0) {
                List<int> newIndices = new();
                int minI, minJ = -1, minK = -1, currentStart, currentCount;
                float minSqrDistance = float.MaxValue, currentSqrDistance;
                List<int> innerCurveIndices = Enumerable.Range(0, innerCurves.Count).ToList();
                while (innerCurveIndices.Count > 0) {
                    currentStart = totalPoints.Count;
                    currentCount = currentStart;
                    newIndices.Clear();
                    // Find inner curve closest to the outer one.
                    minSqrDistance = float.MaxValue;
                    minI = -1;
                    minK = -1;
                    minJ = -1;
                    for (k = 0; k < totalIndices.Count; k++) {
                        for (i = 0; i < innerCurveIndices.Count; i++) {
                            for (j = 0; j < innerCurves[innerCurveIndices[i]].Count; j++) {
                                currentSqrDistance = (innerCurves[innerCurveIndices[i]][j] - totalPoints[totalIndices[k]]).sqrMagnitude;
                                if (currentSqrDistance < minSqrDistance) {
                                    minSqrDistance = currentSqrDistance;
                                    minI = innerCurveIndices[i];
                                    minJ = j;
                                    minK = k;
                                }
                            }
                        }
                    }
                    // found a curve closest to the outer curve - add it
                    // Flip curve if it is clockwise to maintain consistent winding.
                    clockwise = EvaluateCurveWindingInPlane(innerCurves[minI], upDirection);
                    if (clockwise)
                        innerCurves[minI].Reverse();
                    // Add points themselves
                    prevPoint = innerCurves[minI][innerCurves[minI].Count - 1];
                    for (j = 0; j < innerCurves[minI].Count; j++) {
                        // skip duplicates
                        if ((innerCurves[minI][j] - prevPoint).sqrMagnitude < Epsilon)
                            continue;
                        prevPoint = innerCurves[minI][j];
                        newIndices.Add(currentCount);
                        currentCount++;
                        totalPoints.Add(innerCurves[minI][j] + upDirection * upOffset);
                    }
                    // Insert indicies correctly into the outercurve.
                    totalIndices.InsertRange(minK + 1, newIndices.Rotate(minJ));
                    totalIndices.Insert(minK + innerCurves[minI].Count + 1, currentStart + minJ);
                    totalIndices.Insert(minK + innerCurves[minI].Count + 2, totalIndices[minK]);
                    innerCurveIndices.Remove(minI);
                }
            }

            // Now have simple clockwise polygon - apply the Ear Clipping Algorythm.
            int a, b, c, failCount;
            int currentIndex, nextIndex, previousIndex, checkIndex;
            Vector3 currentVertex, nextVertex, previousVertex, sidePrevious, sideNext, cross, sideClosing, inPlaneNormalPrevious, inPlaneNormalNext, inPlaneNormalClosing, checkVertex, checkVector;
            float checkAllignment;
            bool found;
            // Prepare with previous side so it becomes current in the loop
            a = (totalIndices.Count - 1);
            b = 0;
            currentIndex = totalIndices[a];
            nextIndex = totalIndices[b];
            currentVertex = totalPoints[currentIndex];
            nextVertex = totalPoints[nextIndex];
            sideNext = nextVertex - currentVertex;
            inPlaneNormalNext = Vector3.Cross(upDirection, sideNext);
            failCount = 0;
            while (totalIndices.Count > 3 && failCount < totalIndices.Count) {
                // Indicies from index map
                c = a % totalIndices.Count;
                a = b % totalIndices.Count;
                b = (a + 1) % totalIndices.Count;
                // indicies from vertex list
                previousIndex = currentIndex;
                currentIndex = nextIndex;
                nextIndex = totalIndices[b];
                // sides
                previousVertex = currentVertex;
                currentVertex = nextVertex;
                nextVertex = totalPoints[nextIndex];
                sidePrevious = -sideNext;
                sideNext = nextVertex - currentVertex;
                inPlaneNormalPrevious = inPlaneNormalNext;
                inPlaneNormalNext = Vector3.Cross(upDirection, sideNext);
                sideClosing = nextVertex - previousVertex;
                inPlaneNormalClosing = Vector3.Cross(sideClosing, upDirection);
                // determine angle
                cross = Vector3.Cross(sideNext, sidePrevious);
                checkAllignment = Vector3.Dot(cross, upDirection);
                if (checkAllignment == 0) {
                    // angle is equal to 180 degrees or duplicate point
                    // TODO: if happens - also remove from vertex array
                    totalIndices.RemoveAt(a);
                    // Shift indicies as they both might have wrapped around and be affected by vertex removal
                    if (c > a) // account for cyclic indices
                        c = totalIndices.Count + c - 1;
                    if (b > a) // account for cyclic indices
                        b = totalIndices.Count + b - 1;
                    // update variables from which previous will be taken in next iteration
                    a = c;
                    currentIndex = previousIndex;
                    currentVertex = previousVertex;
                    sideNext = sideClosing;
                    inPlaneNormalNext = -inPlaneNormalClosing;
                    continue;
                }
                if (checkAllignment < 0) {
                    // angle is greater then 180 degrees - not an ear
                    failCount++;
                    continue;
                }
                // angle is less than 180 degrees - an ear
                // check if contains other vertices
                found = false;
                for (i = 0; i < totalIndices.Count; i++) {
                    // skip check for current points
                    if (i == b || i == a || i == c)
                        continue;
                    checkIndex = totalIndices[i];
                    // skip if they map to the same point
                    if (checkIndex == previousIndex || checkIndex == currentIndex || checkIndex == nextIndex)
                        continue;
                    checkVertex = totalPoints[checkIndex];
                    // Compare vertex to each side:
                    // check against next side
                    checkVector = checkVertex - currentVertex;
                    checkAllignment = Vector3.Dot(inPlaneNormalNext, checkVector);
                    if (checkAllignment < 0) // on the outer side - skip to next
                        continue;
                    // check against previous side
                    checkAllignment = Vector3.Dot(inPlaneNormalPrevious, checkVector);
                    if (checkAllignment < 0) // on the outer side - skip to next
                        continue;
                    // check against closing side
                    checkVector = checkVertex - nextVertex;
                    checkAllignment = Vector3.Dot(inPlaneNormalClosing, checkVector);
                    if (checkAllignment < 0) // on the outer side - skip to next
                        continue;
                    // vertex is in the triangle
                    found = true;
                    break;
                }
                if (found) { // There is a vertex in the ear - skip to next point
                    failCount++;
                    continue;
                }
                failCount = 0;
                // No verticies inside - add triangle, remove current vertex and update loop variables
                totalTriangles.Add(currentIndex);
                totalTriangles.Add(nextIndex);
                totalTriangles.Add(previousIndex);
                totalIndices.RemoveAt(a);
                // Shift indicies as they both might have wrapped around and be affected by vertex removal
                if (c > a) // account for cyclic indices
                    c = totalIndices.Count + c - 1;
                if (b > a) // account for cyclic indices
                    b = totalIndices.Count + b - 1;
                // update variables from which previous will be taken in next iteration
                a = c;
                currentIndex = previousIndex;
                currentVertex = previousVertex;
                sideNext = sideClosing;
                inPlaneNormalNext = -inPlaneNormalClosing;
            }
            // NB! Check if failed to finalize the mesh - for now just accept and try to continue.
            if (totalIndices.Count > 3 && failCount == totalIndices.Count)
                throw new Exception("Failed to triangulate curve");
            else {
                // Add Remaining triangle
                totalTriangles.Add(totalIndices[0]);
                totalTriangles.Add(totalIndices[1]);
                totalTriangles.Add(totalIndices[2]);
            }
            MeshData meshData = new MeshData(totalPoints.Count, totalTriangles.Count, true, true);

            // Set Vertex Data.
            // Recalculate UVs within boundary in top half of UV space
            Vector2 uv;
            for (i = 0; i < totalPoints.Count; i++) {
                temp = reorient.MultiplyPoint3x4(totalPoints[i]);
                uv = new Vector2(
                    Mathf.InverseLerp(minMaxU.x, minMaxU.y, temp.x),
                    Mathf.Lerp(0.5f, 1f, Mathf.InverseLerp(minMaxV.x, minMaxV.y, temp.z))
                );
                meshData.SetVertex(i, totalPoints[i],
                    uv, upDirection);
            }
            // Set Triangle Data.
            for (i = 0; i < totalTriangles.Count / 3; i++) {
                meshData.SetTriangle(i * 3,
                    totalTriangles[i * 3], totalTriangles[i * 3 + 1], totalTriangles[i * 3 + 2]
                );
            }
            return meshData;
        }
    }
}