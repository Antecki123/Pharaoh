using Controllers.Ai;
using Controllers.Ai.Strategy;
using Models.Ai;
using Models.Construction;
using Models.Settler;
using UnityEngine;
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

            movementHandler = new NpcMovementHandler(navigationGraph, constructionGrid, settlerModel.SettlerDefinition.MovementSpeed);
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
}