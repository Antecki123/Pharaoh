using Controllers.Construction;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Gameplay
{
    [CreateAssetMenu(fileName = "ScenarioData", menuName = "Game Configs/ScenarioData")]
    public class ScenarioData : ScriptableObject
    {
        public string ScenarioName;

        public int BaseGold;

        [HideInInspector] public List<BuildingAvailibility> AvailableBuildings;
    }

    [Serializable]
    public class BuildingAvailibility
    {
        public BuildingDefinition buildingDefinition;
        public bool isAvailable;
    }
}