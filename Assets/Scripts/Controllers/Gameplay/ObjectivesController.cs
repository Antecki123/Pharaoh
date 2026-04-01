using Models.Gameplay;
using System;
using Zenject;

namespace Controllers.Gameplay
{
    public class ObjectivesController : IInitializable, ITickable
    {
        private readonly ObjectivesModel objectivesModel;
        private readonly ScenarioModel scenarioModel;

        private readonly ReachPopulationObjective.Factory populationObjectiveFactory;
        private readonly GatherGoldObjective.Factory gatherGoldObjectiveFactory;
        private readonly GatherCommodityObjective.Factory gatherCommodityObjectiveFactory;
        private readonly BuildBuildingObjective.Factory buildBuildingObjectiveFactory;

        public ObjectivesController(ObjectivesModel objectivesModel, ScenarioModel scenarioModel,
            ReachPopulationObjective.Factory populationObjectiveFactory, GatherGoldObjective.Factory gatherGoldObjectiveFactory,
            GatherCommodityObjective.Factory gatherCommodityObjectiveFactory, BuildBuildingObjective.Factory buildBuildingObjectiveFactory)
        {
            this.objectivesModel = objectivesModel;
            this.scenarioModel = scenarioModel;
            this.populationObjectiveFactory = populationObjectiveFactory;
            this.gatherGoldObjectiveFactory = gatherGoldObjectiveFactory;
            this.gatherCommodityObjectiveFactory = gatherCommodityObjectiveFactory;
            this.buildBuildingObjectiveFactory = buildBuildingObjectiveFactory;
        }

        public void Initialize()
        {
            SetObjectives();
        }

        public void Tick()
        {
            if (objectivesModel.Objectives.Count <= 0)
                return;

            foreach (var objective in objectivesModel.Objectives)
            {
                objective.Process();
            }
        }

        private void SetObjectives()
        {
            if (!scenarioModel.IsScenarioLoaded)
                return;

            foreach (var objective in scenarioModel.Scenario.Objectives)
            {
                switch (objective)
                {
                    case ReachPopulationObjectiveDefinition def:
                        objectivesModel.AddObjective(
                            populationObjectiveFactory.Create(def));
                        break;

                    case GatherGoldObjectiveDefinition def:
                        objectivesModel.AddObjective(
                            gatherGoldObjectiveFactory.Create(def));
                        break;

                    case GatherCommodityObjectiveDefinition def:
                        objectivesModel.AddObjective(
                            gatherCommodityObjectiveFactory.Create(def));
                        break;

                    case BuildBuildingObjectiveDefinition def:
                        objectivesModel.AddObjective(
                            buildBuildingObjectiveFactory.Create(def));
                        break;

                    default:
                        throw new Exception($"Unknown objective type {objective.GetType()}");
                }
            }
        }
    }
}