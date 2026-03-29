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
        private StrategyFactory strategyFactory;

        public SettlerState SettlerState = SettlerState.Idle;

        [Inject]
        public void Constructor(NavigationGraph navigationGraph, ConstructionGrid constructionGrid, StrategyFactory strategyFactory)
        {
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;
            this.strategyFactory = strategyFactory;
        }

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;

            movementHandler = new NpcMovementHandler(navigationGraph, constructionGrid, settlerModel.SettlerDefinition.MovementSpeed);
        }

        public void InitAiStrategy()
        {
            strategy = strategyFactory.GetStrategy(this, StrategyDefinition.Settler);
        }

        public void Tick()
        {
            viewDebug.Update(settlerModel);
            strategy?.Tick();
        }
    }
}