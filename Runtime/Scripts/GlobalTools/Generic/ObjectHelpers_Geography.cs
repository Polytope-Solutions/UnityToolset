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
                geoTileIndex.y += gameTileIndex.y;
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
                Vector2Int gameTileIndex = new Vector2Int(
                    Mathf.RoundToInt(worldCoordinate.x / tileSize(zoomLevel)),
                    Mathf.RoundToInt(worldCoordinate.z / tileSize(zoomLevel))
                );
                return gameTileIndex;
            }
            public static Vector2Int World2GeoTile(int zoomLevel, Vector3 worldCoordinate, Vector2 baseGeoCoordinate, TileSize tileSize) {
                Vector2Int geoTileIndex = GameTile2GeoTile(zoomLevel, World2GameTile(zoomLevel, worldCoordinate, tileSize), baseGeoCoordinate);
                return geoTileIndex;
            }
        }
    }
}
