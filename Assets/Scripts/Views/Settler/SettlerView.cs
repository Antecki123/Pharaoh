using Controllers.Ai.Strategy;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Settler;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Zenject;

namespace Views.Settler
{
    [SelectionBase]
    public class SettlerView : MonoBehaviour
    {
        public SettlerModel SettlerModel => settlerModel;

        public NpcMovementHandler MovementHandler => movementHandler;

        [Space] public PlayerViewDebug viewDebug;

        private SettlerModel settlerModel;
        private Strategy strategy;
        private NpcMovementHandler movementHandler;

        [Inject] private NavigationGraph navigationGraph;

        public bool IsBuisy = false;

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;

            movementHandler = new NpcMovementHandler(navigationGraph);
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

    [System.Serializable]
    public class PlayerViewDebug
    {
        public float Rest;
        public float Entertainment;
        public float Pray;
        public float Health;
        [Space]
        public string strategyState;

        private readonly Dictionary<SettlerStrategyState, string> stateNames = new()
        {
            { SettlerStrategyState.Relocation, "Relocation" },
            { SettlerStrategyState.Resting, "Resting" },
            { SettlerStrategyState.Working, "Working" },
            { SettlerStrategyState.Leasure, "Leasure" },
            { SettlerStrategyState.Praying, "Praying" },
            { SettlerStrategyState.Healing, "Healing" },
        };

        public void Update(SettlerModel settlerModel)
        {
            Rest = settlerModel.SettlerNeeds.Rest.Value;
            Entertainment = settlerModel.SettlerNeeds.Entertainment.Value;
            Pray = settlerModel.SettlerNeeds.Pray.Value;
            Health = settlerModel.SettlerNeeds.Health.Value;

            strategyState = stateNames[settlerModel.StrategyState];
        }
    }
}

public class NpcMovementHandler
{
    public bool RequiredMovement => waypoints?.Count > 0 && currentIndex < waypoints.Count;
    public Vector3 TargetPosition => waypoints[currentIndex];

    private readonly NavigationGraph navigationGraph;

    public List<Vector3> waypoints = new List<Vector3>();
    public int currentIndex = 0;

    public NpcMovementHandler(NavigationGraph navigationGraph)
    {
        this.navigationGraph = navigationGraph;
    }

    public bool CalculateRoute(Vector3 startPoint, Vector3 endPoint)
    {
        var nodesList = navigationGraph.Nodes;
        var startNode = navigationGraph.GetNode(startPoint);
        var goalNode = navigationGraph.GetNode(endPoint);

        currentIndex = 0;
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