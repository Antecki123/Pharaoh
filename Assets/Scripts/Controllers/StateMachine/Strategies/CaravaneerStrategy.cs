using Models.Ai.Pathfinding;
using UnityEngine;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class CaravaneerStrategy : Strategy
    {
        public CaravaneerStrategy(SettlerView context, IPathfindingBrain<Vector2> pathfinding, Animator animator)
        {
            aiBrain = new AiBrain();


        }
    }
}