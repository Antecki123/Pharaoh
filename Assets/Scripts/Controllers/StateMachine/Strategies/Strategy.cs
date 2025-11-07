using System;
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
        private readonly SettlerView settlerView;

        public StrategyFactory(SettlerView settlerView)
        {
            this.settlerView = settlerView;
        }

        public Strategy GetStrategy(StrategyDefinition strategyDefinition)
        {
            return strategyDefinition switch
            {
                StrategyDefinition.Immigrant => new ImmigrantStrategy(settlerView),
                StrategyDefinition.Settler => new SettlerStrategy(settlerView),
                _ => throw new ArgumentException($"Unknown strategy type: {strategyDefinition}")
            };
        }
    }

    public enum StrategyDefinition
    {
        Immigrant,
        Settler
    }
}