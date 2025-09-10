using Models.Ai.Pathfinding;
using UnityEngine;
using Views.Road;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class CaravaneerStrategy : Strategy
    {
        public CaravaneerStrategy(SettlerView context, IPathfindingBrain<RoadNode> pathfinding, Animator animator)
        {
            aiBrain = new AiBrain();


        }
    }
}