using Models.Settler;
using System.Collections.Generic;
using UnityEngine;

namespace Views.Settler
{
    [System.Serializable]
    public class PlayerViewDebug
    {
        public float Work;
        public float Rest;
        //public float Entertainment;
        //public float Pray;
        //public float Health;
        [Space]
        public string strategyState;

        private readonly Dictionary<SettlerStrategyState, string> stateNames = new()
        {
            { SettlerStrategyState.Idle, "Idle" },
            { SettlerStrategyState.Resting, "Resting" },
            { SettlerStrategyState.Working, "Working" },
            { SettlerStrategyState.Leasure, "Leasure" },
            { SettlerStrategyState.Praying, "Praying" },
            { SettlerStrategyState.Healing, "Healing" },
        };

        public void Update(SettlerModel settlerModel)
        {
            Work = settlerModel.SettlerNeeds.Work.Value;
            Rest = settlerModel.SettlerNeeds.Rest.Value;
            //Entertainment = settlerModel.SettlerNeeds.Entertainment.Value;
            //Pray = settlerModel.SettlerNeeds.Pray.Value;
            //Health = settlerModel.SettlerNeeds.Health.Value;

            strategyState = stateNames[settlerModel.StrategyState];
        }
    }
}