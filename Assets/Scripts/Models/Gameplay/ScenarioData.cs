using Controllers.Construction;
using Controllers.SceneManagment;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Gameplay
{
    [CreateAssetMenu(fileName = "ScenarioData", menuName = "Game Configs/ScenarioData")]
    public class ScenarioData : ScriptableObject
    {
        public SceneName Scenario;

        public int Mission;

        public string ScenarioName;

        public int BaseGold;

        [HideInInspector] public List<BuildingAvailibility> AvailableBuildings;

        [SerializeReference] public List<Objective> Objectives;
    }

    [Serializable]
    public class BuildingAvailibility
    {
        public BuildingDefinition buildingDefinition;
        public bool isAvailable;
    }
}