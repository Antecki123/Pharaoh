using App.Signals;
using Controllers.Ai.Strategy;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Economy;
using Models.Settler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Views.Settler
{
    [SelectionBase]
    public class SettlerView : MonoBehaviour
    {
        public SettlerModel SettlerModel => settlerModel;

        [SerializeField] private Animator animator;

        private SettlerModel settlerModel;
        private Strategy strategy;

        private SignalBus signalBus;
        private NavigationGraph navigationGraph;
        private HabitationModel habitationModel;

        [Inject]
        public void Constructor(SignalBus signalBus, NavigationGraph navigationGraph, HabitationModel habitationModel)
        {
            this.signalBus = signalBus;
            this.navigationGraph = navigationGraph;
            this.habitationModel = habitationModel;
        }

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;

            var strategyFactory = new StrategyFactory(this, animator);
            strategy = strategyFactory.GetStrategy(StrategyDefinition.Idle);

            StartCoroutine(FindPathToHome());
        }

        int currentIndex = 0;
        List<Vector3> waypoints = new List<Vector3>();
        float movementSpeed = 1.8f;

        public void Tick()
        {
            if (waypoints.Count == 0)
                return;

            var targetPos = waypoints[currentIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);

            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 15f);
            }

            if (Vector3.Distance(transform.position, targetPos) <= 0.1f)
            {
                currentIndex = (currentIndex + 1) % waypoints.Count;
            }

            if (currentIndex == waypoints.Count - 1)
            {
                movementSpeed = 0f;
                gameObject.SetActive(false);
                //Destroy(gameObject);
            }

            //strategy?.Tick();
        }

        private IEnumerator FindPathToHome()
        {
            yield return null;

            var nodesList = navigationGraph.Nodes.ToList();
            var startNode = nodesList[Random.Range(0, nodesList.Count)];
            var endNodePosition = habitationModel.Habitations[settlerModel.Habitation].EntranceTransform.position;
            var endNode = navigationGraph.GetNode(endNodePosition);

            transform.position = startNode.Data;

            var dStar = new DStarLite<Vector3>();
            dStar.Initialize(nodesList, startNode, endNode);
            var path = dStar.GetPath();

            foreach (var position in path)
                waypoints.Add(position.Data);
        }

        private void OnDestroy()
        {
            signalBus.Fire(new SettlersSignals.DespawnSettler(this));
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var waypoint in waypoints)
            {
                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), waypoint, Quaternion.identity, .25f, EventType.Repaint);
            }
        }
    }
}