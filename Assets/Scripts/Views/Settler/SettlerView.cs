using Controllers.Ai.Strategy;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Construction;
using Models.Settler;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Views.Construction;
using Zenject;

namespace Views.Settler
{
    public enum SettlerState
    {
        Idle,
        Busy,
        Movement
    }

    [SelectionBase]
    public class SettlerView : MonoBehaviour
    {
        public SettlerModel SettlerModel => settlerModel;

        public NpcMovementHandler MovementHandler => movementHandler;

        [Space] public PlayerViewDebug viewDebug;

        private SettlerModel settlerModel;
        private Strategy strategy;
        private NpcMovementHandler movementHandler;

        private NavigationGraph navigationGraph;
        private ConstructionGrid constructionGrid;

        public SettlerState SettlerState = SettlerState.Idle;

        [Inject]
        public void Constructor(NavigationGraph navigationGraph, ConstructionGrid constructionGrid)
        {
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;
        }

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;

            movementHandler = new NpcMovementHandler(navigationGraph, constructionGrid);
        }

        public void InitAiStrategy()
        {
            var strategyFactory = new StrategyFactory(this);
            strategy = strategyFactory.GetStrategy(StrategyDefinition.Settler);
        }

        public void Tick()
        {
            viewDebug.Update(settlerModel);
            strategy?.Tick();
        }
    }

    public class NpcMovementHandler
    {
        public IReadOnlyList<Vector3> Waypoints => waypoints;
        public bool RequiredMovement => waypoints?.Count > 0 && CurrentIndex < waypoints.Count;
        public Vector3 NextPosition => waypoints.Count > 0 ? waypoints[CurrentIndex] : Vector3.zero;
        public Vector3 TargetPosition => waypoints.Count > 0 ? waypoints[^1] : Vector3.zero;
        public int CurrentIndex { get; set; } = 0;

        private readonly NavigationGraph navigationGraph;
        private readonly ConstructionGrid constructionGrid;

        private List<Vector3> waypoints = new List<Vector3>();

        public NpcMovementHandler(NavigationGraph navigationGraph, ConstructionGrid constructionGrid)
        {
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;
        }

        public bool CalculateRoute(BuildingView origin, BuildingView target)
        {
            if (!constructionGrid.HasRoadConnection(origin) || !constructionGrid.HasRoadConnection(target))
                return false;

            var nodesList = navigationGraph.Nodes;

            var cellSize = 4f;
            var originRoadTile = constructionGrid.GetConnectedRoadTile(origin);
            var startNode = navigationGraph.GetClosestNode(new Vector3(originRoadTile.x * cellSize, 0f, originRoadTile.y * cellSize));

            var targetRoadTile = constructionGrid.GetConnectedRoadTile(target);
            var goalNode = navigationGraph.GetClosestNode(new Vector3(targetRoadTile.x * cellSize, 0f, targetRoadTile.y * cellSize));

            if (startNode == null || goalNode == null)
                return false;

            CurrentIndex = 0;
            waypoints.Clear();

            var dStar = new DStarLite<Vector3>();
            dStar.Initialize(nodesList, startNode, goalNode);
            var path = dStar.GetPath();

            if (path.Count == 0)
                Debug.LogWarning($"[D*Lite] Cannot find a path between {startNode.Data} and {goalNode.Data}.");

            waypoints = path.ConvertAll(p => new Vector3(p.Data.x, p.Data.y, p.Data.z));

            return waypoints.Count > 0;
        }

        [BurstCompile]
        public struct MovementJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<float3> targetPositions;
            [ReadOnly] public NativeArray<float> movementSpeeds;
            [ReadOnly] public float deltaTime;

            public void Execute(int index, TransformAccess transform)
            {
                float3 currentPos = transform.position;
                float3 targetPos = targetPositions[index];
                float distance = math.distance(currentPos, targetPos);

                if (distance < 0.01f)
                {
                    transform.position = targetPos;
                    return;
                }

                float t = math.min(movementSpeeds[index] * deltaTime / distance, 1f);
                transform.position = math.lerp(currentPos, targetPos, t);

                float3 direction = targetPos - currentPos;
                direction.y = 0;

                if (math.lengthsq(direction) > 0.001f)
                {
                    direction = math.normalize(direction);
                    quaternion targetRotation = quaternion.LookRotation(direction, math.up());
                    transform.rotation = targetRotation;
                }
            }
        }
    }
}