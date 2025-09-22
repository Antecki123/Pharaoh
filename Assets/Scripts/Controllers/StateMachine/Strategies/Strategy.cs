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
        private readonly Animator animator;

        public StrategyFactory(SettlerView context, Animator animator)
        {
            this.context = context;
            this.animator = animator;
        }

        public Strategy GetStrategy(StrategyDefinition strategyDefinition)
        {
            return strategyDefinition switch
            {
                StrategyDefinition.Idle => new SettlerStrategy(context),
                _ => throw new ArgumentException($"Unknown strategy type: {strategyDefinition}")
            };
        }
    }

    public enum StrategyDefinition
    {
        Idle,
        Working,
        Sleep,
        Entertainment,
        Caravaneer,
        Farmer
    }
}