using Models.Ai.Pathfinding;
using System;
using UnityEngine;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public abstract class Strategy
    {
        protected AiBrain aiBrain;

        public void Tick()
        {
            aiBrain.UpdateAction();
        }

        protected void AddTransition(IState to, IState from, Func<bool> condition)
            => aiBrain.AddTransition(to, from, condition);

        protected void AddAnyTransition(IState to, Func<bool> condition)
            => aiBrain.AddAnyTransition(to, condition);
    }

    public class StrategyFactory
    {
        private readonly SettlerView context;
        private readonly IPathfindingBrain<Vector2> pathfinding;
        private readonly Animator animator;

        public StrategyFactory(SettlerView context, IPathfindingBrain<Vector2> pathfinding, Animator animator)
        {
            this.context = context;
            this.pathfinding = pathfinding;
            this.animator = animator;
        }

        public Strategy GetStrategy(StrategyDefinition strategyDefinition)
        {
            return strategyDefinition switch
            {
                StrategyDefinition.CARAVANEER => new CaravaneerStrategy(context, pathfinding, animator),
                //StrategyDefinition.EXPLODER => new ExploderStrategy(context, pathfinding, animator),
                _ => throw new ArgumentException($"Unknown strategy type: {strategyDefinition}")
            };
        }
    }

    public enum StrategyDefinition
    {
        NONE,
        CARAVANEER
    }
}