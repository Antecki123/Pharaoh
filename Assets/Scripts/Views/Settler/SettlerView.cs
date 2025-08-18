using Controllers.Ai.Strategy;
using Models.Ai.Pathfinding;
using Models.Settler;
using UnityEngine;

namespace Views.Settler
{
    [SelectionBase]
    public class SettlerView : MonoBehaviour
    {
        private SettlerModel settlerModel;
        private Strategy strategy;
        private IPathfindingBrain<Vector2> pathfinding;

        private Vector3 currentTarget = new Vector3(5f, 0, 5f);

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;

            //strategy = new StrategyFactory(this, pathfinding, null).GetStrategy(StrategyDefinition.CARAVANEER);
        }

        public void Tick()
        {
            //strategy?.Tick();

            if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
                return;

            MoveTowardsTarget();
        }

        private void MoveTowardsTarget()
        {
            Vector3 dir = (currentTarget - transform.position).normalized;
            transform.position += settlerModel.MovementSpeed * Time.deltaTime * dir;
        }
    }
}