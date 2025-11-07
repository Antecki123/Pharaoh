using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Economy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class SleepState : IState
    {
        private readonly SettlerView settlerView;
        private readonly NavigationGraph navigationGraph;
        private readonly HabitationModel habitation;

        private DStarLite<Vector3> dStar;

        private List<Vector3> waypoints = new List<Vector3>();
        private int currentIndex = 0;
        private bool reachedTarget = false;

        public SleepState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
            navigationGraph = ProjectContext.Instance.Container.Resolve<NavigationGraph>();
            habitation = ProjectContext.Instance.Container.Resolve<HabitationModel>();

            dStar = new DStarLite<Vector3>();
        }

        public void OnEnter()
        {
            reachedTarget = false;
            waypoints.Clear();
            currentIndex = 0;

            var currentLocationTransform = settlerView.SettlerModel.CurrentLocation.EntranceTransform;
            settlerView.transform.SetPositionAndRotation(currentLocationTransform.position, currentLocationTransform.rotation);
            settlerView.gameObject.SetActive(true);

            if (settlerView.SettlerModel.Habitation != null)
            {
                waypoints = CalculateRoute(currentLocationTransform.position, habitation.Habitations[settlerView.SettlerModel.Habitation].EntranceTransform.position);
            }
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = false;
            settlerView.gameObject.SetActive(false);
        }

        public void Tick()
        {
            Profiler.BeginSample("Settler.SleepState.Tick");
            if (reachedTarget)
            {
                settlerView.SettlerModel.StrategyState = StrategyState.Sleeping;
                settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = true;
            }
            else
            {
                settlerView.SettlerModel.StrategyState = StrategyState.GoToSleep;
                GoToLocation();
            }
            Profiler.EndSample();
        }

        public void FixedTick()
        {

        }

        private List<Vector3> CalculateRoute(Vector3 startPoint, Vector3 endPoint)
        {
            var nodesList = navigationGraph.Nodes;
            var startNode = navigationGraph.GetNode(startPoint);
            var goalNode = navigationGraph.GetNode(endPoint);

            dStar = new DStarLite<Vector3>();
            dStar.Initialize(nodesList, startNode, goalNode);

            var path = dStar.GetPath();

            if (path.Count == 0)
                Debug.LogWarning($"[D*Lite] Cannot find a path between {startNode.Data} and {goalNode.Data}.");

            return path.ConvertAll(p => new Vector3(p.Data.x, p.Data.y, p.Data.z));
        }

        private void GoToLocation()
        {
            if (waypoints.Count == 0 || currentIndex >= waypoints.Count)
                return;

            var targetPos = waypoints[currentIndex];
            settlerView.transform.position = Vector3.MoveTowards(settlerView.transform.position, targetPos, settlerView.SettlerModel.SettlerDefinition.MovementSpeed * Time.deltaTime);

            var direction = (targetPos - settlerView.transform.position).normalized;
            if (direction.sqrMagnitude > 0.0001f)
            {
                settlerView.transform.forward = Vector3.Lerp(settlerView.transform.forward, direction, Time.deltaTime * 15f);
            }

            if (Vector3.Distance(settlerView.transform.position, targetPos) <= 0.1f)
            {
                currentIndex++;

                if (currentIndex >= waypoints.Count)
                {
                    reachedTarget = true;
                    settlerView.SettlerModel.CurrentLocation = habitation.Habitations[settlerView.SettlerModel.Habitation];
                    settlerView.gameObject.SetActive(false);
                }
            }
        }
    }
}