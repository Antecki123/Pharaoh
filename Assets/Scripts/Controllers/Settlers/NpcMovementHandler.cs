using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Construction;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Views.Construction;

namespace Controllers.Ai
{
    public class NpcMovementHandler
    {
        public IReadOnlyList<Vector3> Waypoints => waypoints;
        public bool RequiredMovement => waypoints?.Count > 0 && CurrentIndex < waypoints.Count;
        public Vector3 NextPosition => waypoints.Count > 0 ? waypoints[CurrentIndex] : Vector3.zero;
        public Vector3 TargetPosition => waypoints.Count > 0 ? waypoints[^1] : Vector3.zero;
        public float MovementSpeed => movementSpeed;

        public int CurrentIndex { get; set; } = 0;

        private readonly NavigationGraph navigationGraph;
        private readonly ConstructionGrid constructionGrid;

        private List<Vector3> waypoints = new List<Vector3>();
        private float movementSpeed;

        public NpcMovementHandler(NavigationGraph navigationGraph, ConstructionGrid constructionGrid, float movementSpeed = 2.5f)
        {
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;
            this.movementSpeed = movementSpeed;
        }

        public bool CalculateRoute(BuildingView origin, BuildingView target)
        {
            if (!constructionGrid.HasRoadConnection(origin) || !constructionGrid.HasRoadConnection(target))
                return false;

            var startNode = navigationGraph.GetClosestNode(origin.transform.position);
            var goalNode = navigationGraph.GetClosestNode(target.transform.position);

            if (startNode == null || goalNode == null)
                return false;

            CurrentIndex = 0;
            waypoints.Clear();

            var dStar = new DStarLite<Vector3>();
            dStar.Initialize(navigationGraph.Nodes, startNode, goalNode);
            var path = dStar.GetPath();

            if (path.Count == 0)
                Debug.LogWarning($"[D*Lite] Cannot find a path between {startNode.Data} and {goalNode.Data}.");

            waypoints = path.ConvertAll(p => new Vector3(p.Data.x, p.Data.y, p.Data.z));

            return waypoints.Count > 0;
        }

        public bool CalculateRoute(Vector3 origin, Vector3 target)
        {
            var startNode = navigationGraph.GetClosestNode(origin, 4f);
            var goalNode = navigationGraph.GetClosestNode(target, 4f);

            if (startNode == null || goalNode == null)
                return false;

            CurrentIndex = 0;
            waypoints.Clear();

            var dStar = new DStarLite<Vector3>();
            dStar.Initialize(navigationGraph.Nodes, startNode, goalNode);
            var path = dStar.GetPath();

            if (path.Count == 0)
                Debug.LogWarning($"[D*Lite] Cannot find a path between {startNode.Data} and {goalNode.Data}.");

            waypoints = path.ConvertAll(p => new Vector3(p.Data.x, p.Data.y, p.Data.z));

            return waypoints.Count > 0;
        }

        public void ModifySpeed(float movementSpeed)
        {
            this.movementSpeed = movementSpeed;
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