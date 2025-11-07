using Controllers.Ai.Strategy;
using Models.Settler;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Views.Settler
{
    [SelectionBase]
    public class SettlerView : MonoBehaviour
    {
        public SettlerModel SettlerModel => settlerModel;

        [Header("DEBUG")]
        public PlayerViewDebug viewDebug;

        private SettlerModel settlerModel;
        private Strategy strategy;

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;
        }

        public void InitAiStrategy()
        {
            var strategyFactory = new StrategyFactory(this);
            strategy = strategyFactory.GetStrategy(StrategyDefinition.Settler);
        }

        public void Tick()
        {
            settlerModel?.SettlerNeeds?.UpdateNeeds();
            viewDebug.Update(settlerModel);

            Profiler.BeginSample("Settler.UpdateBehavior");
            strategy?.Tick();
            Profiler.EndSample();
        }
    }

    [System.Serializable]
    public class PlayerViewDebug
    {
        public float Rest;
        public float Entertainment;
        public float Pray;
        public float Health;
        [Space]
        public string strategyState;

        private Dictionary<StrategyState, string> stateNames = new()
        {
            { StrategyState.GoToSleep, "GoToSleep" },
            { StrategyState.Sleeping, "Sleeping" },
            { StrategyState.GoToWork, "GoToWork" },
            { StrategyState.Working, "Working" },
        };

        public void Update(SettlerModel settlerModel)
        {
            //Rest = settlerModel.SettlerNeeds.Rest.Value;
            //Entertainment = settlerModel.SettlerNeeds.Entertainment.Value;
            //Pray = settlerModel.SettlerNeeds.Pray.Value;
            //Health = settlerModel.SettlerNeeds.Health.Value;

            strategyState = stateNames[settlerModel.StrategyState];
        }
    }

    public enum StrategyState
    {
        GoToSleep,
        Sleeping,
        GoToWork,
        Working
    }
}