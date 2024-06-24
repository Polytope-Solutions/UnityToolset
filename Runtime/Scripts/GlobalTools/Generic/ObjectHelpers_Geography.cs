using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
    public static class Geography {
        /* 
            * Supported coordinate systems:
            *  - Geo coordinates (horizontal: longitude, and vertical: latitude, noth in fractional degrees);
            *  - Geo tile (zoom level ordered tiles used for slippy map APIs);
            *      NB! order horizontally: left to write, vertically: up to down (fliped from gaming 3D convension).
            *      NB! when converted to geo coordinate returns top-left corner of the tile.
            *      NB! fractional part referes to percentage within tile from top-left corner
            *  - Game tiles (same as Geo tile indices but relative to tile of some base coordinate);
            *      NB! indicies are going from base coordinate positive to the right and up, and negative in opposite directions.
            *      NB! when converted to game coordinates base tile is adjusted to account for offset of base coordinate inside the base tile (keeping base coordinate at 0,0,0).
            *      NB! when converted to game coordinates return bottom-left of tile position.
            *      NB! fractional part referes to percentage within tile from bottom-left corner.
            *  - Flat game coordinates (in game coordinates relative to some base coordinate);
            *      NB! base coordinate is at Game coordinate 0,0,0.
            *      NB! scaled through providing a function (see GameTileSize delegate) that returns the tile size in game units based on zoom level.
            *  - Spherical game coordinates (in absolute orientation with 0,0 being -forward axis);
            *  - Game UVs;
            *      NB! UVs follow convention of bottom-left as 0,0 and top-right as 1,1.
            *  
            *  All intermediate conversions account for axis flips and offsets.
            *  
            *  Additionally has:
            *  - Extra constatnts for geo-related calculations.
            *  
            *  TODO: further reduce cross-calls between converters.
            *  TODO: switch to using doubles for all coordinates.
            */

        public delegate double GameTileSize(int zoomLevel);
        public static readonly double EarthCircumference = 2 * 6378137.0d * Math.PI;
        public static readonly double EarthRadius = 6372.7982d;
        public static readonly double GeoMaxLatitude = 85.05112878d;
        public static readonly double GeoMaxLongitude = 180d;
        public static double GeoTileWidthMeters(int zoomLevel) => Geography.EarthCircumference / Math.Pow(2, zoomLevel);
        public static double GeoTileWidthDegrees(int zoomLevel) => (Geography.GeoMaxLongitude * 2) / Math.Pow(2, zoomLevel);

        // Geo coordinate converters
        public static void Geo2GeoTile(this Vector3Double geoCoordinate, int zoomLevel, ref Vector3Double result) {
            result.x = ((double)geoCoordinate.x + 180.0d) / 360.0d * (1 << zoomLevel);
            result.z = (1.0d - Math.Log(Math.Tan(geoCoordinate.z * Math.PI / 180.0d) +
                    1.0d / Math.Cos(geoCoordinate.z * Math.PI / 180.0d)) / Math.PI) / 2.0d * (1 << zoomLevel);
        }
        public static void Geo2FlatGame(this Vector3Double geoCoordinate, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            geoCoordinate.Geo2GeoTile(zoomLevel, ref result);
            result.GeoTile2FlatGame(zoomLevel, baseGeoTile, gameTileSize, ref result);
        }
        public static void Geo2SphericalGame(this Vector3Double geoCoordinate, Vector3Double center, double radius, Matrix4x4 correction, ref Vector3Double result) {
            result = center
                + new Vector3Double(
                    correction.MultiplyVector(
                        Quaternion.AngleAxis(-geoCoordinate.xf, Vector3.up) *
                        (
                            Quaternion.AngleAxis(geoCoordinate.zf, Vector3.right)
                            * -Vector3.forward
                        )
                    )
                ) * radius;
        }
        public static void Geo2GameTile(this Vector3Double geoCoordinate, int zoomLevel, Vector3Double baseGeoTile, ref Vector3Double result) {
            geoCoordinate.Geo2GeoTile(zoomLevel, ref result);
            result.GeoTile2GameTile(baseGeoTile, ref result);
        }
        public static void Geo2GameUV(this Vector3Double geoCoordinate, int zoomLevel, ref Vector3Double result) {
            geoCoordinate.Geo2GeoTile(zoomLevel, ref result);
            result.GeoTile2GameUV(ref result);
        }

        // GeoTile converters
        public static void GeoTileParent(this Vector3Double geoTile, ref Vector3Double result) {
            result = geoTile / 2;
        }
        public static void GeoTileChildren(this Vector3Double geoTile, ref Vector3Double[] result) {
            if (result == null || result.Length != 4)
                result = new Vector3Double[4];
            Vector3Double geoTileFloored = geoTile.FloorToInt();
            result[0] = geoTileFloored * 2 + Vector3Double.zero;
            result[1] = geoTileFloored * 2 + Vector3Double.right;
            result[2] = geoTileFloored * 2 + Vector3Double.forward;
            result[3] = geoTileFloored * 2 + Vector3Double.right + Vector3Double.forward;
        }
        public static void GeoTileSiblings(this Vector3Double geoTile, ref Vector3Double[] result) {
            if (result == null || result.Length != 3)
                result = new Vector3Double[3];
            Vector3Double geoTileFloored = geoTile.FloorToInt();
            Vector3Double parent = Vector3Double.zero;
            geoTile.GeoTileParent(ref parent);
            if (parent.x.Fraction() >= 0.5f && parent.z.Fraction() >= 0.5f) {
                result[0] = geoTileFloored - Vector3Double.right;
                result[1] = geoTileFloored - Vector3Double.forward;
                result[2] = geoTileFloored - Vector3Double.right - Vector3Double.forward;
            }
            else if (parent.x.Fraction() < 0.5f && parent.z.Fraction() >= 0.5f) {
                result[0] = geoTileFloored + Vector3Double.right;
                result[1] = geoTileFloored - Vector3Double.forward;
                result[2] = geoTileFloored + Vector3Double.right - Vector3Double.forward;
            }
            else if (parent.x.Fraction() < 0.5f && parent.z.Fraction() < 0.5f) {
                result[0] = geoTileFloored + Vector3Double.right;
                result[1] = geoTileFloored + Vector3Double.forward;
                result[2] = geoTileFloored + Vector3Double.right + Vector3Double.forward;
            }
            else {
                result[0] = geoTileFloored - Vector3Double.right;
                result[1] = geoTileFloored + Vector3Double.forward;
                result[2] = geoTileFloored - Vector3Double.right + Vector3Double.forward;
            }
        }
        public static void GeoTile2Geo(this Vector3Double geoTile, int zoomLevel, ref Vector3Double result) {
            double n = Math.PI - ((2.0d * Math.PI * geoTile.z) / (1 << zoomLevel));
            result.x = geoTile.x / (1 << zoomLevel) * 360.0d - 180.0d;
            result.z = 180.0d / Math.PI * Math.Atan(Math.Sinh(n));
        }
        public static void GeoTile2FlatGame(this Vector3Double geoTile, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            double zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = (-baseGeoTile.x + geoTile.x) * zoomGameTileSize;
            result.z = (baseGeoTile.z - geoTile.z) * zoomGameTileSize;
        }
        public static void GeoTile2SphericalGame(this Vector3Double geoTile, int zoomLevel, Vector3Double center, double radius, Matrix4x4 correction, ref Vector3Double result) {
            geoTile.GeoTile2Geo(zoomLevel, ref result);
            result.Geo2SphericalGame(center, radius, correction, ref result);
        }
        public static void GeoTile2GameTile(this Vector3Double geoTile, Vector3Double baseGeoTile, ref Vector3Double result) {
            result.x = -Math.Floor(baseGeoTile.x) + geoTile.x;
            result.z = Math.Ceiling(baseGeoTile.z) - geoTile.z + 1;
        }
        public static void GeoTile2GameUV(this Vector3Double geoTile, ref Vector3Double result) {
            result.x = geoTile.x.Fraction();
            result.z = 1 - geoTile.z.Fraction();
        }
        public static void GeoTile2GeoTileIndex(this Vector3Double geoTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(geoTile.xf);
            result.z = Mathf.FloorToInt(geoTile.zf);
        }
        public static void GeoTileIndex2Geo(this Vector3Int geoTileIndex, int zoomLevel, ref Vector3Double result) {
            (new Vector3Double(geoTileIndex + Vector3.right * 0.5f + Vector3.forward * 0.5f)).GeoTile2Geo(zoomLevel, ref result);
        }
        public static void GeoTileIndex2FlatGame(this Vector3Int geoTileIndex, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            (new Vector3Double(geoTileIndex + Vector3.right * 0.5f + Vector3.forward * 0.5f)).GeoTile2FlatGame(zoomLevel, baseGeoTile, gameTileSize, ref result);
        }
        public static void GeoTileIndex2SphericalGame(this Vector3Int geoTileIndex, int zoomLevel, Vector3Double center, double radius, Matrix4x4 correction, ref Vector3Double result) {
            geoTileIndex.GeoTileIndex2Geo(zoomLevel, ref result);
            result.Geo2SphericalGame(center, radius, correction, ref result);
        }
        public static void GeoTileIndex2GameTileIndex(this Vector3Int geoTileIndex, Vector3Double baseGeoTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(-Mathf.FloorToInt(baseGeoTile.xf) + geoTileIndex.x + 0.5f);
            result.z = Mathf.FloorToInt(Mathf.FloorToInt(baseGeoTile.zf) - geoTileIndex.z + 0.5f);
        }

        // Flat game coordinate converters
        public static void FlatGame2GameTile(this Vector3Double flatGameCoordinate, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            // Calculate game tile index accounting for the in-game offset of base tile.
            double zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = baseGeoTile.x.Fraction() + flatGameCoordinate.x / zoomGameTileSize;
            result.z = -baseGeoTile.z.Fraction() + 1f + flatGameCoordinate.z / zoomGameTileSize;
        }
        public static void FlatGame2GeoTile(this Vector3Double flatGameCoordinate, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            // Round down game tile to index relative to base offset.
            double zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = baseGeoTile.x + flatGameCoordinate.x / zoomGameTileSize;
            result.z = baseGeoTile.z - flatGameCoordinate.z / zoomGameTileSize;
        }
        public static void FlatGame2Geo(this Vector3Double flatGameCoordinate, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            flatGameCoordinate.FlatGame2GeoTile(zoomLevel, baseGeoTile, gameTileSize, ref result);
            result.GeoTile2Geo(zoomLevel, ref result);
        }
        public static void FlatGame2GameUV(this Vector3Double flatGameCoordinate, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            flatGameCoordinate.FlatGame2GameTile(zoomLevel, baseGeoTile, gameTileSize, ref result);
            result.GameTile2GameUV(ref result);
        }

        // Spherical game coordinate converters
        public static void SphericalGame2GameTile(this Vector3Double sphericalGameCoordinate, Vector3Double center, int zoomLevel, Vector3Double baseGeoTile, Matrix4x4 inverseCorrection, ref Vector3Double result) {
            sphericalGameCoordinate.SphericalGame2Geo(center, inverseCorrection, ref result);
            result.Geo2GameTile(zoomLevel, baseGeoTile, ref result);
        }
        public static void SphericalGame2GeoTile(this Vector3Double sphericalGameCoordinate, Vector3Double center, int zoomLevel, Matrix4x4 inverseCorrection, ref Vector3Double result) {
            sphericalGameCoordinate.SphericalGame2Geo(center, inverseCorrection, ref result);
            result.Geo2GeoTile(zoomLevel, ref result);
        }
        public static void SphericalGame2Geo(this Vector3Double sphericalGameCoordinate, Vector3Double center, Matrix4x4 inverseCorrection, ref Vector3Double result) {
            // Check if axis is vertical - then just give (0, 90) or (0, -90).
            Vector3 offsetNormalized = inverseCorrection.MultiplyVector((sphericalGameCoordinate - center).normalized.ToVector3());
            float verticalAllignment = Vector3.Dot(offsetNormalized, Vector3.up);
            if (Mathf.Abs(Mathf.Abs(verticalAllignment) - 1f) < float.Epsilon) {
                result = Vector3Double.up * Mathf.Sign(verticalAllignment) * 90f;
                return;
            }
            // Compute normal to plane with vertical axis.
            Vector3 verticalPlaneNormal = Vector3.Cross(Vector3.up, offsetNormalized);
            // Compute horizontal projection of coordinate.
            Vector3 horizontalAxis = Vector3.Cross(verticalPlaneNormal, Vector3.up);
            // Compute angles within the vertical plane for y and within projected horizontal plane for x
            float anglePitch = -Vector3.SignedAngle(horizontalAxis, offsetNormalized, verticalPlaneNormal);
            float angleYaw = -Vector3.SignedAngle(-Vector3.forward, horizontalAxis, Vector3.up);

            result.x = angleYaw;
            result.z = anglePitch;
        }
        public static void SphericalGame2GameUV(this Vector3Double sphericalGameCoordinate, Vector3Double center, int zoomLevel, Matrix4x4 inverseCorrection, ref Vector3Double result) {
            sphericalGameCoordinate.SphericalGame2Geo(center, inverseCorrection, ref result);
            result.Geo2GameUV(zoomLevel, ref result);
        }

        // GameTile converters
        public static void GameTile2Geo(this Vector3Double gameTile, int zoomLevel, Vector3Double baseGeoTile, ref Vector3Double result) {
            gameTile.GameTile2GeoTile(baseGeoTile, ref result);
            result.GeoTile2Geo(zoomLevel, ref result);
        }
        public static void GameTile2GeoTile(this Vector3Double gameTile, Vector3Double baseGeoTile, ref Vector3Double result) {
            result.x = Math.Floor(baseGeoTile.x) + gameTile.x;
            result.z = Math.Floor(baseGeoTile.z) - gameTile.z + 1;
        }
        public static void GameTile2FlatGame(this Vector3Double gameTile, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            double zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = (-baseGeoTile.x.Fraction() + gameTile.x) * zoomGameTileSize;
            result.z = (baseGeoTile.z.Fraction() - 1 + gameTile.z) * zoomGameTileSize;
        }
        public static void GameTile2SphericalGame(this Vector3Double gameTile, Vector3Double center, int zoomLevel, Vector3Double baseGeoTile, double radius, Matrix4x4 correction, ref Vector3Double result) {
            gameTile.GameTile2Geo(zoomLevel, baseGeoTile, ref result);
            result.Geo2SphericalGame(center, radius, correction, ref result);
        }
        public static void GameTile2GameUV(this Vector3Double gameTile, ref Vector3Double result) {
            result.x = gameTile.x.Fraction();
            result.z = gameTile.z.Fraction();
            // Handle negative tile indices
            result.x = (result.x > 0) ? result.x : 1 + result.x;
            result.z = (result.z > 0) ? result.z : 1 + result.z;
        }
        public static void GameTile2GameTileIndex(this Vector3Double gameTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(gameTile.xf);
            result.z = Mathf.FloorToInt(gameTile.zf);
        }
        public static void GameTileIndex2Geo(this Vector3Int gameTileIndex, int zoomLevel, Vector3Double baseGeoTile, ref Vector3Double result) {
            (new Vector3Double(gameTileIndex + Vector3.right * 0.5f + Vector3.forward * 0.5f))
                .GameTile2Geo(zoomLevel, baseGeoTile, ref result);
        }
        public static void GameTileIndex2GeoTileIndex(this Vector3Int gameTileIndex, Vector3Double baseGeoTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(Mathf.FloorToInt(baseGeoTile.xf) + gameTileIndex.x + 0.5f);
            result.z = Mathf.FloorToInt(Mathf.FloorToInt(baseGeoTile.zf) - gameTileIndex.z + 0.5f);
        }
        public static void GameTileIndex2FlatGame(this Vector3Int gameTileIndex, int zoomLevel, Vector3Double baseGeoTile, GameTileSize gameTileSize, ref Vector3Double result) {
            (new Vector3Double(gameTileIndex + Vector3.right * 0.5f + Vector3.forward * 0.5f))
                .GameTile2FlatGame(zoomLevel, baseGeoTile, gameTileSize, ref result);
        }
        public static void GameTileIndex2SphericalGame(this Vector3Int gameTileIndex, int zoomLevel, Vector3Double baseGeoTile, Vector3Double center, float radius, Matrix4x4 correction, ref Vector3Double result) {
            gameTileIndex.GameTileIndex2Geo(zoomLevel, baseGeoTile, ref result);
            result.Geo2SphericalGame(center, radius, correction, ref result);
        }
    }
}
