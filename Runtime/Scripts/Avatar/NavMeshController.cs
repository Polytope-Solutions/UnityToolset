using System.Collections.Generic;
using UnityEngine;

#if ALLOW_AI_NAVIGATION
using UnityEngine.AI;
#endif

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    public class NavMeshController : MonoBehaviour {
        #if ALLOW_AI_NAVIGATION
        [SerializeField] protected NavMeshAgent agent;
        protected NavMeshBuildSettings settings;

        protected List<NavMeshBuildSource> buildSources = new();
        protected Bounds generationBounds = new Bounds();
        #endif

        protected static readonly int OBSTACLE = 1 << 0;

        protected virtual void Start() {
            #if ALLOW_AI_NAVIGATION
            this.agent = FindAnyObjectByType<NavMeshAgent>();
            InitSettings(this.agent, ref this.settings);
            #endif
        }
        #if ALLOW_AI_NAVIGATION
        protected static void InitSettings(NavMeshAgent agent, ref NavMeshBuildSettings settings) {
            if (NavMesh.GetSettingsCount() == 0)
                settings = NavMesh.CreateSettings();
            else if (agent != null)
                settings = NavMesh.GetSettingsByID(agent.agentTypeID);
            else
                settings = NavMesh.GetSettingsByIndex(0);
        }
        #endif
        public void GenerateNavMesh(List<(Bounds worldBounds, Quaternion orientation)> floorData,
                List<(Bounds worldBounds, Quaternion ortientation)> obstacleData = null,
                bool autoClean = true) {
            #if ALLOW_AI_NAVIGATION
            if (autoClean)
                NavMesh.RemoveAllNavMeshData();
            this.buildSources.Clear();
            this.generationBounds = new Bounds();
            // Create floor as passable area
            GeometryToBuildData(floorData, 0, ref buildSources, ref generationBounds);
            // Create obstacles
            GeometryToBuildData(obstacleData, OBSTACLE, ref buildSources, ref generationBounds);

            // build navmesh
            NavMeshData built = NavMeshBuilder.BuildNavMeshData(
                settings, this.buildSources,
                //new Bounds(Vector3.zero, floorSize),
                //floorCenter, Quaternion.identity
                generationBounds,
                Vector3.zero, Quaternion.identity
            );
            NavMesh.AddNavMeshData(built);
            #endif
        }
        #if ALLOW_AI_NAVIGATION
        protected static void GeometryToBuildData(List<(Bounds worldBounds, Quaternion orientation)> data, int areaType,
                ref List<NavMeshBuildSource> buildSources, ref Bounds generationBounds) {
            NavMeshBuildSource item;
            foreach ((Bounds worldBounds, Quaternion ortientation) itemRaw in data) {
                item = new NavMeshBuildSource {
                    transform = Matrix4x4.TRS(itemRaw.worldBounds.center,
                        itemRaw.ortientation, Vector3.one),
                    shape = NavMeshBuildSourceShape.Box,
                    size = itemRaw.worldBounds.size,
                    area = areaType
                };
                generationBounds.Encapsulate(itemRaw.worldBounds);
                buildSources.Add(item);
            }
        }
        #endif
    }
}
