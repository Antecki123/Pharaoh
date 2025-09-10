using Controllers.Ai.Strategy;
using Models.Ai.Pathfinding;
using Models.Settler;
using UnityEngine;
using Views.Road;

namespace Views.Settler
{
    [SelectionBase]
    public class SettlerView : MonoBehaviour
    {
        private SettlerModel settlerModel;
        private Strategy strategy;
        private IPathfindingBrain<RoadNode> pathfinding;

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;

            //strategy = new StrategyFactory(this, pathfinding, null).GetStrategy(StrategyDefinition.None);
        }

        public void Tick()
        {
            strategy?.Tick();
        }
    }
}