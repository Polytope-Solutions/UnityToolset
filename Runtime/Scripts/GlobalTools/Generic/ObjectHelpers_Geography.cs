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

        public delegate float GameTileSize(int zoomLevel);
        public static readonly float EarthCircumference = 2 * 6378137.0f * Mathf.PI;
        public static readonly float EarthRadius = 6372.7982f;
        public static readonly float GeoMaxLatitude = 85.05112878f;
        public static readonly float GeoMaxLongitude = 180f;
        public static float GeoTileWidthMeters(int zoomLevel) => Geography.EarthCircumference / Mathf.Pow(2, (float)zoomLevel);
        public static float GeoTileWidthDegrees(int zoomLevel) => (Geography.GeoMaxLongitude * 2) / Mathf.Pow(2, (float)zoomLevel);

        // Geo coordinate converters
        public static void Geo2GeoTile(this Vector3 geoCoordinate, int zoomLevel, ref Vector3 result) {
            result.x = (float)(((double)geoCoordinate.x + 180.0d) / 360.0d * (1 << zoomLevel));
            result.z = (float)((1.0d - Math.Log(Math.Tan((double)geoCoordinate.z * Math.PI / 180.0d) +
                    1.0d / Math.Cos((double)geoCoordinate.z * Math.PI / 180.0d)) / Math.PI) / 2.0d * (1 << zoomLevel));
        }
        public static void Geo2FlatGame(this Vector3 geoCoordinate, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            geoCoordinate.Geo2GeoTile(zoomLevel, ref result);
            result.GeoTile2FlatGame(zoomLevel, baseGeoTile, gameTileSize, ref result);
        }
        public static void Geo2SphericalGame(this Vector3 geoCoordinate, Vector3 center, float radius, ref Vector3 result) {
            result = center + Quaternion.AngleAxis(-geoCoordinate.x, Vector3.up) *
                    (Quaternion.AngleAxis(geoCoordinate.z, Vector3.right)
                        * -Vector3.forward * radius
                    );
        }
        public static void Geo2GameTile(this Vector3 geoCoordinate, int zoomLevel, Vector3 baseGeoTile, ref Vector3 result) {
            geoCoordinate.Geo2GeoTile(zoomLevel, ref result);
            result.GeoTile2GameTile(baseGeoTile, ref result);
        }
        public static void Geo2GameUV(this Vector3 geoCoordinate, int zoomLevel, ref Vector3 result) {
            geoCoordinate.Geo2GeoTile(zoomLevel, ref result);
            result.GeoTile2GameUV(ref result);
        }

        // GeoTile converters
        public static void GeoTileParent(this Vector3 geoTile, ref Vector3 result) {
            result = geoTile / 2f;
        }
        public static void GeoTileChildren(this Vector3 geoTile, ref Vector3[] result) {
            if (result == null || result.Length != 4)
                result = new Vector3[4];
            Vector3 geoTileFloored = geoTile.FloorToInt();
            result[0] = geoTileFloored * 2 + Vector3Int.zero;
            result[1] = geoTileFloored * 2 + Vector3Int.right;
            result[2] = geoTileFloored * 2 + Vector3Int.forward;
            result[3] = geoTileFloored * 2 + Vector3Int.right + Vector3Int.forward;
        }
        public static void GeoTileSiblings(this Vector3 geoTile, ref Vector3[] result) {
            if (result == null || result.Length != 3)
                result = new Vector3[3];
            Vector3 geoTileFloored = geoTile.FloorToInt();
            Vector3 parent = Vector3.zero;
            geoTile.GeoTileParent(ref parent);
            if (parent.x.Fraction() >= 0.5f && parent.z.Fraction() >= 0.5f) {
                result[0] = geoTileFloored - Vector3Int.right;
                result[1] = geoTileFloored - Vector3Int.forward;
                result[2] = geoTileFloored - Vector3Int.right - Vector3Int.forward;
            }
            else if (parent.x.Fraction() < 0.5f && parent.z.Fraction() >= 0.5f) {
                result[0] = geoTileFloored + Vector3Int.right;
                result[1] = geoTileFloored - Vector3Int.forward;
                result[2] = geoTileFloored + Vector3Int.right - Vector3Int.forward;
            }
            else if (parent.x.Fraction() < 0.5f && parent.z.Fraction() < 0.5f) {
                result[0] = geoTileFloored + Vector3Int.right;
                result[1] = geoTileFloored + Vector3Int.forward;
                result[2] = geoTileFloored + Vector3Int.right + Vector3Int.forward;
            }
            else {
                result[0] = geoTileFloored - Vector3Int.right;
                result[1] = geoTileFloored + Vector3Int.forward;
                result[2] = geoTileFloored - Vector3Int.right + Vector3Int.forward;
            }
        }
        public static void GeoTile2Geo(this Vector3 geoTile, int zoomLevel, ref Vector3 result) {
            double n = Math.PI - ((2.0d * Math.PI * (double)geoTile.z) / (1 << zoomLevel));
            result.x = (float)(((double)geoTile.x / (1 << zoomLevel) * 360.0d) - 180.0d);
            result.z = (float)(180.0d / Math.PI * Math.Atan(Math.Sinh(n)));
        }
        public static void GeoTile2FlatGame(this Vector3 geoTile, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            float zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = (-baseGeoTile.x + geoTile.x) * zoomGameTileSize;
            result.z = (baseGeoTile.z - geoTile.z) * zoomGameTileSize;
        }
        public static void GeoTile2SphericalGame(this Vector3 geoTile, int zoomLevel, Vector3 center, float radius, ref Vector3 result) {
            geoTile.GeoTile2Geo(zoomLevel, ref result);
            result.Geo2SphericalGame(center, radius, ref result);
        }
        public static void GeoTile2GameTile(this Vector3 geoTile, Vector3 baseGeoTile, ref Vector3 result) {
            result.x = -Mathf.FloorToInt(baseGeoTile.x) + geoTile.x;
            result.z = Mathf.FloorToInt(baseGeoTile.z) - geoTile.z + 1;
        }
        public static void GeoTile2GameUV(this Vector3 geoTile, ref Vector3 result) {
            result.x = geoTile.x.Fraction();
            result.z = 1 - geoTile.z.Fraction();
        }
        public static void GeoTile2GeoTileIndex(this Vector3 geoTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(geoTile.x);
            result.z = Mathf.FloorToInt(geoTile.z);
        }
        public static void GeoTileIndex2Geo(this Vector3Int geoTileIndex, int zoomLevel, ref Vector3 result) {
            (geoTileIndex + Vector3.right*0.5f + Vector3.forward*0.5f).GeoTile2Geo(zoomLevel, ref result);
        }
        public static void GeoTileIndex2FlatGame(this Vector3Int geoTileIndex, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            (geoTileIndex + Vector3.right * 0.5f + Vector3.forward * 0.5f).GeoTile2FlatGame(zoomLevel, baseGeoTile, gameTileSize, ref result);
        }
        public static void GeoTileIndex2SphericalGame(this Vector3Int geoTileIndex, int zoomLevel, Vector3 center, float radius, ref Vector3 result) {
            geoTileIndex.GeoTileIndex2Geo(zoomLevel, ref result);
            result.Geo2SphericalGame(center, radius, ref result);
        }
        public static void GeoTileIndex2GameTileIndex(this Vector3Int geoTileIndex, Vector3 baseGeoTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(-Mathf.FloorToInt(baseGeoTile.x) + geoTileIndex.x + 0.5f);
            result.z = Mathf.FloorToInt(Mathf.FloorToInt(baseGeoTile.z) - geoTileIndex.z + 0.5f);
        }
        
        // Flat game coordinate converters
        public static void FlatGame2GameTile(this Vector3 flatGameCoordinate, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            // Calculate game tile index accounting for the in-game offset of base tile.
            float zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = baseGeoTile.x.Fraction() + flatGameCoordinate.x / zoomGameTileSize;
            result.z = -baseGeoTile.z.Fraction() + 1f + flatGameCoordinate.z / zoomGameTileSize;
        }
        public static void FlatGame2GeoTile(this Vector3 flatGameCoordinate, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            // Round down game tile to index relative to base offset.
            float zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = baseGeoTile.x + flatGameCoordinate.x / zoomGameTileSize;
            result.z = baseGeoTile.z - flatGameCoordinate.z / zoomGameTileSize;
        }
        public static void FlatGame2Geo(this Vector3 flatGameCoordinate, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            flatGameCoordinate.FlatGame2GeoTile(zoomLevel, baseGeoTile, gameTileSize, ref result);
            result.GeoTile2Geo(zoomLevel, ref result);
        }
        public static void FlatGame2GameUV(this Vector3 flatGameCoordinate, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            flatGameCoordinate.FlatGame2GameTile(zoomLevel, baseGeoTile, gameTileSize, ref result);
            result.GameTile2GameUV(ref result);
        }
        
        // Spherical game coordinate converters
        public static void SphericalGame2GameTile(this Vector3 sphericalGameCoordinate, Vector3 center, int zoomLevel, Vector3 baseGeoTile, ref Vector3 result) {
            sphericalGameCoordinate.SphericalGame2Geo(center, ref result);
            result.Geo2GameTile(zoomLevel, baseGeoTile, ref result);
        }
        public static void SphericalGame2GeoTile(this Vector3 sphericalGameCoordinate, Vector3 center, int zoomLevel, ref Vector3 result) {
            sphericalGameCoordinate.SphericalGame2Geo(center, ref result);
            result.Geo2GeoTile(zoomLevel, ref result);
        }
        public static void SphericalGame2Geo(this Vector3 sphericalGameCoordinate, Vector3 center, ref Vector3 result) {
            // Check if axis is vertical - then just give (0, 90) or (0, -90).
            float verticalAllignment = Vector3.Dot((sphericalGameCoordinate - center).normalized, Vector3.up);
            if (Mathf.Abs(Mathf.Abs(verticalAllignment) - 1f) < float.Epsilon) { 
                result = Mathf.Sign(verticalAllignment) * Vector3.up * 90f;
                return;
            }
            // Compute normal to plane with vertical axis.
            Vector3 verticalPlaneNormal = Vector3.Cross(Vector3.up, (sphericalGameCoordinate - center));
            // Compute horizontal projection of coordinate.
            Vector3 horizontalAxis = Vector3.Cross(verticalPlaneNormal, Vector3.up);
            // Compute angles within the vertical plane for y and within projected horizontal plane for x
            float anglePitch = -Vector3.SignedAngle(horizontalAxis, (sphericalGameCoordinate - center), verticalPlaneNormal);
            float angleYaw = -Vector3.SignedAngle(-Vector3.forward, horizontalAxis, Vector3.up);

            result.x = angleYaw;
            result.z = anglePitch;
        }
        public static void SphericalGame2GameUV(this Vector3 sphericalGameCoordinate, Vector3 center, int zoomLevel, ref Vector3 result) {
            sphericalGameCoordinate.SphericalGame2Geo(center, ref result);
            result.Geo2GameUV(zoomLevel, ref result);
        }

        // GameTile converters
        public static void GameTile2Geo(this Vector3 gameTile, int zoomLevel, Vector3 baseGeoTile, ref Vector3 result) {
            gameTile.GameTile2GeoTile(baseGeoTile, ref result);
            result.GeoTile2Geo(zoomLevel, ref result);
        }
        public static void GameTile2GeoTile(this Vector3 gameTile, Vector3 baseGeoTile, ref Vector3 result) {
            result.x = Mathf.FloorToInt(baseGeoTile.x) + gameTile.x; 
            result.z = Mathf.FloorToInt(baseGeoTile.z) - gameTile.z + 1;
        }
        public static void GameTile2FlatGame(this Vector3 gameTile, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            float zoomGameTileSize = gameTileSize(zoomLevel);
            result.x = (-baseGeoTile.x.Fraction() + gameTile.x) * zoomGameTileSize;
            result.z = (baseGeoTile.z.Fraction() - 1 + gameTile.z) * zoomGameTileSize;
        }
        public static void GameTile2SphericalGame(this Vector3 gameTile, Vector3 center, int zoomLevel, Vector3 baseGeoTile, float radius, ref Vector3 result) {
            gameTile.GameTile2Geo(zoomLevel, baseGeoTile, ref result);
            result.Geo2SphericalGame(center, radius, ref result);
        }
        public static void GameTile2GameUV(this Vector3 gameTile, ref Vector3 result) {
            result.x = gameTile.x.Fraction();
            result.z = gameTile.z.Fraction();
            // Handle negative tile indices
            result.x = (result.x > 0) ? result.x : 1 + result.x;
            result.z = (result.z > 0) ? result.z : 1 + result.z;
        }
        public static void GameTile2GameTileIndex(this Vector3 gameTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(gameTile.x);
            result.z = Mathf.FloorToInt(gameTile.z);
        }
        public static void GameTileIndex2Geo(this Vector3Int gameTileIndex, int zoomLevel, Vector3 baseGeoTile, ref Vector3 result) {
            (gameTileIndex + Vector3.right * 0.5f + Vector3.forward * 0.5f)
                .GameTile2Geo(zoomLevel, baseGeoTile, ref result);
        }
        public static void GameTileIndex2GeoTileIndex(this Vector3Int gameTileIndex, Vector3 baseGeoTile, ref Vector3Int result) {
            result.x = Mathf.FloorToInt(Mathf.FloorToInt(baseGeoTile.x) + gameTileIndex.x + 0.5f);
            result.z = Mathf.FloorToInt(Mathf.FloorToInt(baseGeoTile.z) - gameTileIndex.z + 0.5f);
        }
        public static void GameTileIndex2FlatGame(this Vector3Int gameTileIndex, int zoomLevel, Vector3 baseGeoTile, GameTileSize gameTileSize, ref Vector3 result) {
            (gameTileIndex + Vector3.right * 0.5f + Vector3.forward * 0.5f)
                .GameTile2FlatGame(zoomLevel, baseGeoTile, gameTileSize, ref result);
        }
        public static void GameTileIndex2SphericalGame(this Vector3Int gameTileIndex, int zoomLevel, Vector3 baseGeoTile, Vector3 center, float radius, ref Vector3 result) {
            gameTileIndex.GameTileIndex2Geo(zoomLevel, baseGeoTile, ref result);
            result.Geo2SphericalGame(center, radius, ref result);
        }
    }
}
