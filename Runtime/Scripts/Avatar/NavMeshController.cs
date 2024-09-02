using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    public class NavMeshController : MonoBehaviour {
        [SerializeField] private NavMeshAgent agent;
        private const int OBSTACLE = 1 << 0;
        public void GenerateNavMesh((Bounds worldBounds, Quaternion orientation) floorData,
                List<(Bounds worldBounds, Quaternion ortientation)> obstacles = null, 
                bool autoClean=true) {
            if (autoClean)
                NavMesh.RemoveAllNavMeshData();
            NavMeshBuildSettings settings;
            if (NavMesh.GetSettingsCount() == 0)
                settings = NavMesh.CreateSettings();
            else if (this.agent != null)
                settings = NavMesh.GetSettingsByID(this.agent.agentTypeID);
            else
                settings = NavMesh.GetSettingsByIndex(0);
            List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();
            Vector3 floorCenter = floorData.worldBounds.center.XZ().ToXZ();
            Vector3 floorSize = floorData.worldBounds.size.XZ().ToXZ();

            // create floor as passable area
            var floor = new NavMeshBuildSource {
                transform = Matrix4x4.TRS(floorCenter, Quaternion.identity, Vector3.one),
                shape = NavMeshBuildSourceShape.Box,
                size = floorSize
            };
            buildSources.Add(floor);

            // Create obstacle 
            foreach ((Bounds worldBounds, Quaternion ortientation) obstacleData in obstacles) { 
                var obstacle = new NavMeshBuildSource {
                    transform = Matrix4x4.TRS(obstacleData.worldBounds.center,// - floorData.worldBounds.center, 
                         obstacleData.ortientation, Vector3.one),//Quaternion.Inverse(floorData.orientation) *
                    shape = NavMeshBuildSourceShape.Box,
                    size = obstacleData.worldBounds.size,
                    area = OBSTACLE
                };
                buildSources.Add(obstacle);
            }

            // build navmesh
            NavMeshData built = NavMeshBuilder.BuildNavMeshData(
                settings, buildSources, new Bounds(Vector3.zero, floorSize),
                floorCenter, floorData.orientation);
            NavMesh.AddNavMeshData(built);
        }
    }
}
