using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
    public static partial class ObjectHelpers {
        public static class Geography {
            /* 
             * Supported coordinate systems:
             *  - Geo coordinates (horizontal: longitude, and vertical: latitude, noth in fractional degrees);
             *  - Geo tile indices (zoom level ordered tiles used for slippy map APIs);
             *      NB! order horizontally: left to write, vertically: up to down (fliped from gaming 3D convension).
             *      NB! when converted to geo coordinate returns topleft corner of the tile.
             *  - Game tiles (same as Geo tile indices but relative to tile of some base coordinate);
             *      NB! indicies are going from base coordinate positive to the right and up, and negative in opposite directions.
             *      NB! when converted to game coordinates base tile is adjusted to account for offset of base coordinate inside the base tile (keeping base coordinate at 0,0,0).
             *      NB! when converted to game coordinates return center of tile position.
             *  - Game coordinates (in game coordinates relative to some base coordinate);
             *      NB! base coordinate is at Game coordinate 0,0,0.
             *      NB! scaled through providing a function (see GameTileSize delegate) that returns the tile size in game units based on zoom level.
             *      
             *  All intermediate conversions account for axis flips and offsets.
             *  
             *  Additionally has:
             *  - UV calculation within geo tile (based on geo coordinate) and game tile (based on game coordinate).
             *      NB! both UVs follow convention of bottom left as 0,0 and top right as 1,1.
             *  - Extra constatnts for geo-related calculations.
             *  
             *  TODO: simplify and reduce cross-calls between converters.
             *  TODO: double check geo tile vertical size.
             */

            public delegate float GameTileSize(int zoomLevel);
            public static readonly float EarthCircumference = 2 * 6378137.0f * Mathf.PI;
            public static readonly float EarthRadius = 6372.7982f;
            private static float GeoMaxLatitude = 85.05112878f;
            private static float GeoMaxLongitude = 180f;
            //private static float GeoTileSizeMeters(int zoomLevel) => Geography.EarthCircumference / Mathf.Pow(2, (float)zoomLevel);
            private static float GeoTileWidthDegrees(int zoomLevel) => (Geography.GeoMaxLongitude * 2) / Mathf.Pow(2, (float)zoomLevel);
            //private static float GeoTileHeightDegrees(int zoomLevel) => (Geography.GeoMaxLatitude * 4) / Mathf.Pow(2, (float)zoomLevel);
            private static Vector2 GeoTileSizeDegrees(int zoomLevel, Vector2 geoTileIndex) {
                double n1 = Math.PI - ((2.0 * Math.PI * geoTileIndex.y) / Math.Pow(2.0, zoomLevel));
                double n2 = Math.PI - ((2.0 * Math.PI * (geoTileIndex.y+1)) / Math.Pow(2.0, zoomLevel));
                double y1 = 180.0 / Math.PI * Math.Atan(Math.Sinh(n1));
                double y2 = 180.0 / Math.PI * Math.Atan(Math.Sinh(n2));
                Vector2 size = new Vector2(
                    Geography.GeoTileWidthDegrees(zoomLevel),
                    Mathf.Abs((float)(y1 - y2))
                );
                return size;
            }

            public static Vector2Int Geo2GeoTile(int zoomLevel, float lon, float lat) {
                Vector2Int p = new Vector2Int(
                    Mathf.FloorToInt((float)((lon + 180.0) / 360.0 * (1 << zoomLevel))),
                    Mathf.FloorToInt((float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                    1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoomLevel)))
                );
                return p;
            }
            public static Vector3 Geo2Game(int zoomLevel, Vector2 geoCoordinate, Vector2 baseGeoCooridnate, GameTileSize gameTileSize) { 
                Vector2Int gameTileIndex = Geo2GameTile(zoomLevel, geoCoordinate, baseGeoCooridnate, gameTileSize);
                Vector3 gameTileGame = GameTile2Game(zoomLevel, gameTileIndex, baseGeoCooridnate, gameTileSize);
                Vector2 inGeoTileUV = InGeoTileUV(zoomLevel, geoCoordinate);
                float zoomGameTileSize = gameTileSize(zoomLevel);
                Vector3 gameCoordinate = new Vector3(
                    gameTileGame.x + (inGeoTileUV.x - .5f) * zoomGameTileSize,
                    gameTileGame.y,
                    gameTileGame.z + (inGeoTileUV.y - .5f) * zoomGameTileSize
                );
                return gameCoordinate;
            }
            public static Vector2Int Geo2GameTile(int zoomLevel, Vector2 geoCoordinate, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) { 
                Vector2Int geoTileIndex = Geo2GeoTile(zoomLevel, geoCoordinate.x, geoCoordinate.y);
                Vector2Int baseGeoTileIndex = Geo2GeoTile(zoomLevel, baseGeoCoordinate.x, baseGeoCoordinate.y);
                Vector2Int gameTileIndex = geoTileIndex - baseGeoTileIndex;
                gameTileIndex.y = -gameTileIndex.y;
                return gameTileIndex;
            }
            
            public static Vector2 GeoTile2Geo(int zoomLevel, int geoTileIndexX, int geoTileIndexY) {
                double n = Math.PI - ((2.0 * Math.PI * geoTileIndexY) / Math.Pow(2.0, zoomLevel));
                Vector2 p = new Vector2(
                    (float)((geoTileIndexX / Math.Pow(2.0, zoomLevel) * 360.0) - 180.0),
                    (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n))));

                return p;
            }
            public static Vector3 GeoTile2Game(int zoomLevel, Vector2Int geoTileIndex, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) { 
                Vector2Int gameTileIndex = GeoTile2GameTile(zoomLevel, geoTileIndex, baseGeoCoordinate, gameTileSize);
                Vector3 gameCoordinate = GameTile2Game(zoomLevel, gameTileIndex, baseGeoCoordinate, gameTileSize);
                return gameCoordinate;
            }
            public static Vector2Int GeoTile2GameTile(int zoomLevel, Vector2Int geoTileIndex, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) { 
                Vector2Int baseGeoTileIndex = Geo2GeoTile(zoomLevel, baseGeoCoordinate.x, baseGeoCoordinate.y);
                Vector2Int gameTileIndex = geoTileIndex - baseGeoTileIndex;
                gameTileIndex.y = -gameTileIndex.y;
                return gameTileIndex;
            }
            
            public static Vector2Int Game2GameTile(int zoomLevel, Vector3 gameCoordinate, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
                float zoomGameTileSize = gameTileSize(zoomLevel);
                // Extract base coordinate in-game offset.
                Vector2 baseGameUVOffset = -InGeoTileUV(zoomLevel, baseGeoCoordinate);
                // Calculate game tile index accounting for the in-game offset of base tile.
                Vector2Int gameTileIndex = new Vector2Int(
                    Mathf.FloorToInt(gameCoordinate.x / zoomGameTileSize - baseGameUVOffset.x),
                    Mathf.FloorToInt(gameCoordinate.z / zoomGameTileSize - baseGameUVOffset.y)
                );
                return gameTileIndex;
            }
            public static Vector2Int Game2GeoTile(int zoomLevel, Vector3 gameCoordinate, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) { 
                // Extract game tile index for the game coordinate.
                Vector2Int gameTileIndex = Game2GameTile(zoomLevel, gameCoordinate, baseGeoCoordinate, gameTileSize);
                // Convert game tile index to geo tile index.
                Vector2Int geoTileIndex = GameTile2GeoTile(zoomLevel, gameTileIndex, baseGeoCoordinate);
                return geoTileIndex;
            }
            public static Vector2 Game2Geo(int zoomLevel, Vector3 gameCoordinate, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
                Vector2Int geoTileIndex = Game2GeoTile(zoomLevel, gameCoordinate, baseGeoCoordinate, gameTileSize);
                Vector2 gameTileGeo = GeoTile2Geo(zoomLevel, geoTileIndex.x, geoTileIndex.y);
                Vector2 inGameTileUV = InGameTileUV(zoomLevel, gameCoordinate, baseGeoCoordinate, gameTileSize);
                Vector2 zoomDegreeTileSize = GeoTileSizeDegrees(zoomLevel, geoTileIndex);
                Vector2 geoCoordinate = new Vector2(
                    gameTileGeo.x + inGameTileUV.x * zoomDegreeTileSize.x,
                    gameTileGeo.y + (inGameTileUV.y-1) * zoomDegreeTileSize.y
                );
                return geoCoordinate;
            }
            
            public static Vector2 GameTile2Geo(int zoomLevel, Vector2Int gameTileIndex, Vector2 baseGeoCoordinate) {
                // Calculate geo tile index relative to base tile.
                Vector2Int geoTileIndex = GameTile2GeoTile(zoomLevel, gameTileIndex, baseGeoCoordinate);
                // Convert geo tile index to geo-coordinate.
                Vector2 geoCoordinate = GeoTile2Geo(
                    zoomLevel,
                    geoTileIndex.x,
                    geoTileIndex.y);
                return geoCoordinate;
            }
            public static Vector2Int GameTile2GeoTile(int zoomLevel, Vector2Int gameTileIndex, Vector2 baseGeoCoordinate) {
                // Calculate base coordinate geo tile index.
                Vector2Int geoTileIndex = Geo2GeoTile(
                    zoomLevel,
                    baseGeoCoordinate.x,
                    baseGeoCoordinate.y);
                // Add in-game tile indices.
                geoTileIndex.x += gameTileIndex.x;
                geoTileIndex.y -= gameTileIndex.y;
                return geoTileIndex;
            }
            public static Vector3 GameTile2Game(int zoomLevel, Vector2Int gameTileIndex, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) {
                float zoomGameTileSize = gameTileSize(zoomLevel);
                // Extract base coordinate in-game offset.
                Vector3 baseGameUVOffset = -InGeoTileUV(zoomLevel, baseGeoCoordinate);
                // Calculate game coordinate realtive to base offset.
                Vector3 gameCoordinate = new Vector3(
                    (baseGameUVOffset.x + gameTileIndex.x + .5f) * zoomGameTileSize,
                    0,
                    (baseGameUVOffset.y + gameTileIndex.y + .5f) * zoomGameTileSize);
                return gameCoordinate;
            }
            
            public static Vector2 InGeoTileUV(int zoomLevel, Vector2 geoCoordinate) { 
                Vector2Int geoTileIndex = Geo2GeoTile(zoomLevel, geoCoordinate.x, geoCoordinate.y);
                Vector2 geoTileGeo = GeoTile2Geo(zoomLevel, geoTileIndex.x, geoTileIndex.y);
                Vector2 zoomDegreeTileSize = GeoTileSizeDegrees(zoomLevel, geoTileIndex);
                Vector2 geoTileFractional = new Vector2(
                    (geoCoordinate.x - geoTileGeo.x) / zoomDegreeTileSize.x,
                    1+(geoCoordinate.y - geoTileGeo.y) / zoomDegreeTileSize.y
                );
                return geoTileFractional;
            }
            public static Vector2 InGameTileUV(int zoomLevel, Vector3 gameCoordinate, Vector2 baseGeoCoordinate, GameTileSize gameTileSize) { 
                float zoomGameTileSize = gameTileSize(zoomLevel);
                Vector2Int gameTileIndex = Game2GameTile(zoomLevel, gameCoordinate, baseGeoCoordinate, gameTileSize);
                Vector3 gameTileGame = GameTile2Game(zoomLevel, gameTileIndex, baseGeoCoordinate, gameTileSize);
                Vector2 gameTileFractional = new Vector2(
                    (gameCoordinate.x - gameTileGame.x) / zoomGameTileSize + 0.5f,
                    (gameCoordinate.z - gameTileGame.z) / zoomGameTileSize + 0.5f
                );
                return gameTileFractional;
            }
        }
    }
}
