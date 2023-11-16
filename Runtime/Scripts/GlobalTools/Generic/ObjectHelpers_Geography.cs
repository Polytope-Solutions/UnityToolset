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
            *  - Game coordinates (in game coordinates relative to some base coordinate);
            *      NB! base coordinate is at Game coordinate 0,0,0.
            *      NB! scaled through providing a function (see GameTileSize delegate) that returns the tile size in game units based on zoom level.
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
        public static Vector2 Geo2GeoTile(this Vector2 geoCoordinate, int zoomLevel) {
            Vector2 geoTile = new Vector2(
                (float)(((double)geoCoordinate.x + 180.0d) / 360.0d * (1 << zoomLevel)),
                (float)((1.0d - Math.Log(Math.Tan((double)geoCoordinate.y * Math.PI / 180.0d) +
                    1.0d / Math.Cos((double)geoCoordinate.y * Math.PI / 180.0d)) / Math.PI) / 2.0d * (1 << zoomLevel))
            );
            return geoTile;
        }
        public static Vector3 Geo2Game(this Vector2 geoCoordinate, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            return geoCoordinate.Geo2GeoTile(zoomLevel)
                .GeoTile2Game(zoomLevel, baseGeoCoordinate, gameTileSize);
        }
        public static Vector2 Geo2GameTile(this Vector2 geoCoordinate, int zoomLevel, Vector2 baseGeoCoordinate) { 
            return geoCoordinate.Geo2GeoTile(zoomLevel)
                .GeoTile2GameTile(zoomLevel, baseGeoCoordinate);
        }
        public static Vector2 Geo2GameUV(this Vector2 geoCoordinate, int zoomLevel) {
            return geoCoordinate.Geo2GeoTile(zoomLevel)
                .GeoTile2GameUV();
        }

        // GeoTile converters
        public static Vector2 GeoTileParent(this Vector2 geoTile) {
            return geoTile / 2f;
        }
        public static Vector2Int[] GeoTileChildren(this Vector2 geoTile) {
            return new Vector2Int[] { 
                geoTile.FloorToInt()*2 + Vector2Int.zero,
                geoTile.FloorToInt()*2 + Vector2Int.right,
                geoTile.FloorToInt()*2 + Vector2Int.up,
                geoTile.FloorToInt()*2 + Vector2Int.one
            };
        }
        public static Vector2Int[] GeoTileSiblings(this Vector2 geoTile) {
            Vector2Int[] siblings = new Vector2Int[3];
            Vector2 parent = geoTile.GeoTileParent();
            if (parent.x.Fraction() >= 0.5f && parent.y.Fraction() >= 0.5f) { 
                siblings[0] = geoTile.FloorToInt() - Vector2Int.right;
                siblings[1] = geoTile.FloorToInt() - Vector2Int.up;
                siblings[2] = geoTile.FloorToInt() - Vector2Int.right - Vector2Int.up;
            }
            else if (parent.x.Fraction() < 0.5f && parent.y.Fraction() >= 0.5f) {
                siblings[0] = geoTile.FloorToInt() + Vector2Int.right;
                siblings[1] = geoTile.FloorToInt() - Vector2Int.up;
                siblings[2] = geoTile.FloorToInt() + Vector2Int.right - Vector2Int.up;
            }
            else if (parent.x.Fraction() < 0.5f && parent.y.Fraction() < 0.5f) {
                siblings[0] = geoTile.FloorToInt() + Vector2Int.right;
                siblings[1] = geoTile.FloorToInt() + Vector2Int.up;
                siblings[2] = geoTile.FloorToInt() + Vector2Int.right + Vector2Int.up;
            }
            else {
                siblings[0] = geoTile.FloorToInt() - Vector2Int.right;
                siblings[1] = geoTile.FloorToInt() + Vector2Int.up;
                siblings[2] = geoTile.FloorToInt() - Vector2Int.right + Vector2Int.up;
            }
            return siblings;
        }
        public static Vector2 GeoTile2Geo(this Vector2 geoTile, int zoomLevel) {
            double n = Math.PI - ((2.0d * Math.PI * (double)geoTile.y) / (1 << zoomLevel));
            Vector2 geoCoordinate = new Vector2(
                (float)(((double)geoTile.x / (1 << zoomLevel) * 360.0d) - 180.0d),
                (float)(180.0d / Math.PI * Math.Atan(Math.Sinh(n))));

            return geoCoordinate;
        }
        public static Vector3 GeoTile2Game(this Vector2 geoTile, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            float zoomGameTileSize = gameTileSize(zoomLevel);
            Vector2 baseGeoTile = baseGeoCoordinate.Geo2GeoTile(zoomLevel);
            return new Vector3(
                (-baseGeoTile.x + geoTile.x) * zoomGameTileSize,
                0,
                (baseGeoTile.y - geoTile.y) * zoomGameTileSize);
        }
        public static Vector2 GeoTile2GameTile(this Vector2 geoTile, int zoomLevel, Vector2 baseGeoCoordinate) { 
            Vector2 baseGeoTileIndex = baseGeoCoordinate
                .Geo2GeoTile(zoomLevel)
                .FloorToInt();
            return new Vector2(
                -baseGeoTileIndex.x + geoTile.x,
                baseGeoTileIndex.y - geoTile.y + 1);
        }
        public static Vector2 GeoTile2GameUV(this Vector2 geoTile) { 
            return new Vector2(geoTile.x.Fraction(),
                               1 - geoTile.y.Fraction());
        }
        public static Vector2 GeoTileIndex2Geo(this Vector2Int geoTileIndex, int zoomLevel) {
            return (geoTileIndex + Vector2.one*0.5f).GeoTile2Geo(zoomLevel);
        }
        public static Vector3 GeoTileIndex2Game(this Vector2Int geoTileIndex, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            return (geoTileIndex + Vector2.one*0.5f).GeoTile2Game(zoomLevel, baseGeoCoordinate, gameTileSize);
        }
        public static Vector2Int GeoTileIndex2GameTileIndex(this Vector2Int geoTileIndex, int zoomLevel, Vector2 baseGeoCoordinate) {
            return (geoTileIndex + Vector2.one*0.5f).GeoTile2GameTile(zoomLevel, baseGeoCoordinate).FloorToInt();
        }
        
        // Game coordinate converters
        public static Vector2 Game2GameTile(this Vector3 gameCoordinate, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            float zoomGameTileSize = gameTileSize(zoomLevel);
            Vector2 baseGeoTile = baseGeoCoordinate.Geo2GeoTile(zoomLevel);

            // Calculate game tile index accounting for the in-game offset of base tile.
            return new Vector2(
                baseGeoTile.x.Fraction() + gameCoordinate.x / zoomGameTileSize,
                -baseGeoTile.y.Fraction() + 1f + gameCoordinate.z / zoomGameTileSize
            );
        }
        public static Vector2 Game2GeoTile(this Vector3 gameCoordinate, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            float zoomGameTileSize = gameTileSize(zoomLevel);
            Vector2 baseGeoTile = baseGeoCoordinate.Geo2GeoTile(zoomLevel);

            // Round down game tile to index relative to base offset.
            return new Vector2(
                baseGeoTile.x + gameCoordinate.x / zoomGameTileSize,
                baseGeoTile.y - gameCoordinate.z / zoomGameTileSize
            );
        }
        public static Vector2 Game2Geo(this Vector3 gameCoordinate, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            return gameCoordinate.Game2GeoTile(zoomLevel, baseGeoCoordinate, gameTileSize)
                .GeoTile2Geo(zoomLevel);
        }
        public static Vector2 Game2GameUV(this Vector3 gameCoordinate, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            return gameCoordinate.Game2GameTile(zoomLevel, baseGeoCoordinate, gameTileSize)
                .GameTile2GameUV();
        }
        
        // GameTile converters
        public static Vector2 GameTile2Geo(this Vector2 gameTile, int zoomLevel, Vector2 baseGeoCoordinate) {
            return gameTile.GameTile2GeoTile(zoomLevel, baseGeoCoordinate)
                .GeoTile2Geo(zoomLevel);
        }
        public static Vector2 GameTile2GeoTile(this Vector2 gameTile, int zoomLevel, Vector2 baseGeoCoordinate) {
            Vector2 baseGeoTileIndex = baseGeoCoordinate.Geo2GeoTile(zoomLevel)
                .FloorToInt();
            return new Vector2(baseGeoTileIndex.x + gameTile.x,
                baseGeoTileIndex.y - gameTile.y + 1);
        }
        public static Vector3 GameTile2Game(this Vector2 gameTile, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            float zoomGameTileSize = gameTileSize(zoomLevel);
            Vector2 baseGeoTile = baseGeoCoordinate.Geo2GeoTile(zoomLevel);
            return new Vector3(
                (- baseGeoTile.x.Fraction() + gameTile.x) * zoomGameTileSize,
                0,
                (baseGeoTile.y.Fraction() - 1 + gameTile.y) * zoomGameTileSize);
        }
        public static Vector2 GameTile2GameUV(this Vector2 gameTile) {
            Vector2 uv = new Vector2(gameTile.x.Fraction(),
                               gameTile.y.Fraction());
            // Handle negative tile indices
            uv.x = (uv.x > 0) ? uv.x : 1 + uv.x;
            uv.y = (uv.y > 0) ? uv.y : 1 + uv.y;
            return uv;
        }
        public static Vector2 GameTileIndex2Geo(this Vector2Int gameTileIndex, int zoomLevel, Vector2 baseGeoCoordinate) {
            return (gameTileIndex + Vector2.one*0.5f).GameTile2Geo(zoomLevel, baseGeoCoordinate);
        }
        public static Vector2Int GameTileIndex2GeoTileIndex(this Vector2Int gameTileIndex, int zoomLevel, Vector2 baseGeoCoordinate) {
            return (gameTileIndex + Vector2.one*0.5f).GameTile2GeoTile(zoomLevel, baseGeoCoordinate).FloorToInt();
        }
        public static Vector3 GameTileIndex2Game(this Vector2Int gameTileIndex, int zoomLevel, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
            return (gameTileIndex + Vector2.one*0.5f).GameTile2Game(zoomLevel, baseGeoCoordinate, gameTileSize);
        }
    }
}
