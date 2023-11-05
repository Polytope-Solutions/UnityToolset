using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
    public static partial class ObjectHelpers {
        public static class Geography {
            public delegate float TileSize(int zoomLevel);
            public static readonly float EarthCircumference = 2 * 6378137.0f * Mathf.PI;
            public static readonly float EarthRadius = 6372.7982f;
            private static float TileSizeMeters(int zoomLevel) => Geography.EarthCircumference / Mathf.Pow(2, (float)zoomLevel);
            private static float TileSizeDegrees(int zoomLevel) => 360f / Mathf.Pow(2, (float)zoomLevel);
            public static Vector2Int Geo2GeoTile(int zoomLevel, float lon, float lat) {
                Vector2Int p = new Vector2Int(
                    Mathf.RoundToInt((float)((lon + 180.0) / 360.0 * (1 << zoomLevel))),
                    Mathf.RoundToInt((float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                    1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoomLevel)))
                );
                return p;
            }
            public static Vector2 GeoTile2Geo(int zoomLevel, int tile_x, int tile_y) {
                double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoomLevel));
                Vector2 p = new Vector2(
                    (float)((tile_x / Math.Pow(2.0, zoomLevel) * 360.0) - 180.0),
                    (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n))));

                return p;
            }
            public static Vector2Int GameTile2GeoTile(int zoomLevel, Vector2Int gameTileIndex, Vector2 baseGeoCoordinate) {
                Vector2Int geoTileIndex = Geo2GeoTile(
                    zoomLevel,
                    baseGeoCoordinate.x,
                    baseGeoCoordinate.y);
                geoTileIndex.x += gameTileIndex.x;
                geoTileIndex.y -= gameTileIndex.y;
                return geoTileIndex;
            }
            public static Vector2 GameTile2Geo(int zoomLevel, Vector2Int gameTileIndex, Vector2 baseGeoCoordinate) {
                Vector2Int geoTileIndex = GameTile2GeoTile(zoomLevel, gameTileIndex, baseGeoCoordinate);
                Vector2 geoCoordinate = GeoTile2Geo(
                    zoomLevel,
                    geoTileIndex.x,
                    geoTileIndex.y);
                return geoCoordinate;
            }
            public static Vector3 GameTile2World(int zoomLevel, Vector2Int gameTileIndex, TileSize tileSize) {
                Vector3 worldCoordinate = new Vector3(
                    gameTileIndex.x * tileSize(zoomLevel),
                    0,
                    gameTileIndex.y * tileSize(zoomLevel));
                return worldCoordinate;
            }
            public static Vector2Int World2GameTile(int zoomLevel, Vector3 worldCoordinate, TileSize tileSize) {
                float zoomTileSize = tileSize(zoomLevel);
                Vector2Int gameTileIndex = new Vector2Int(
                    Mathf.RoundToInt(worldCoordinate.x / zoomTileSize),
                    Mathf.RoundToInt(worldCoordinate.z / zoomTileSize)
                );
                return gameTileIndex;
            }
            public static Vector2Int World2GeoTile(int zoomLevel, Vector3 worldCoordinate, Vector2 baseGeoCoordinate, TileSize tileSize) {
                Vector2Int geoTileIndex = GameTile2GeoTile(zoomLevel, 
                    World2GameTile(zoomLevel, worldCoordinate, tileSize), baseGeoCoordinate);
                return geoTileIndex;
            }
            public static Vector2 World2Geo(int zoomLevel, Vector3 worldCoordinate, Vector3 baseGeoCoordinate, TileSize tileSize) {
                float zoomTileSize = tileSize(zoomLevel);
                Vector2 gameTileFractional = new Vector2(
                    worldCoordinate.x / zoomTileSize,
                    worldCoordinate.z / zoomTileSize
                );
                Vector2Int gameTileIndex = new Vector2Int(
                    Mathf.RoundToInt(gameTileFractional.x),
                    Mathf.RoundToInt(gameTileFractional.y)
                );
                gameTileFractional = new Vector2(
                    gameTileFractional.x - (float)gameTileIndex.x,
                    gameTileFractional.y - (float)gameTileIndex.y
                );
                Vector2 geoCoordinate = GameTile2Geo(zoomLevel, gameTileIndex, baseGeoCoordinate);
                geoCoordinate.x += gameTileFractional.x * TileSizeDegrees(zoomLevel);
                geoCoordinate.y += gameTileFractional.y * TileSizeDegrees(zoomLevel);
                return geoCoordinate;
            }
        }
    }
}
