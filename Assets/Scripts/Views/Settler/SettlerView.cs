using Controllers.Ai.Strategy;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Settler;
using System.Collections.Generic;
using UnityEngine;
using Views.Settler;
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

            movementHandler = new NpcMovementHandler(navigationGraph, this);
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
    private readonly SettlerView settlerView;

    private readonly NavigationGraph navigationGraph;
    private DStarLite<Vector3> dStar;

    private List<Vector3> waypoints = new List<Vector3>();
    private int currentIndex = 0;

    public NpcMovementHandler(NavigationGraph navigationGraph, SettlerView settlerView)
    {
        this.navigationGraph = navigationGraph;
        this.settlerView = settlerView;

        dStar = new DStarLite<Vector3>();
    }

    public bool CalculateRoute(Vector3 startPoint, Vector3 endPoint)
    {
        var nodesList = navigationGraph.Nodes;
        var startNode = navigationGraph.GetNode(startPoint);
        var goalNode = navigationGraph.GetNode(endPoint);

        currentIndex = 0;
        waypoints.Clear();

        dStar = new DStarLite<Vector3>();
        dStar.Initialize(nodesList, startNode, goalNode);

        var path = dStar.GetPath();

        if (path.Count == 0)
            Debug.LogWarning($"[D*Lite] Cannot find a path between {startNode.Data} and {goalNode.Data}.");

        waypoints = path.ConvertAll(p => new Vector3(p.Data.x, p.Data.y, p.Data.z));
        return waypoints.Count > 0;
    }

    public void ExecuteMovement()
    {
        if (waypoints.Count == 0 || currentIndex >= waypoints.Count)
            return;

        var targetPos = waypoints[currentIndex];
        settlerView.transform.position = Vector3.MoveTowards(settlerView.transform.position, targetPos, settlerView.SettlerModel.SettlerDefinition.MovementSpeed * Time.deltaTime);

        var direction = (targetPos - settlerView.transform.position).normalized;
        if (direction.sqrMagnitude > 0.0001f)
        {
            settlerView.transform.forward = Vector3.Slerp(settlerView.transform.forward, direction, Time.deltaTime * 15f);
        }

        if (Vector3.Distance(settlerView.transform.position, targetPos) <= 0.1f)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Count)
            {
                settlerView.gameObject.SetActive(false);
            }
        }
    }
}