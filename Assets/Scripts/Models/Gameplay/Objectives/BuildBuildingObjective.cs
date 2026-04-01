using Controllers.Construction;
using Zenject;

namespace Models.Gameplay
{
    public class BuildBuildingObjective : IObjective
    {
        public class Factory : PlaceholderFactory<BuildBuildingObjectiveDefinition, BuildBuildingObjective> { }

        public string Name { get; private set; }

        public bool IsFulfilled { get; private set; }

        public string ProgressDisplay => $"{0}/{count}";

        private BuildingDefinition buildingDefinition;
        private int count;

        public BuildBuildingObjective(BuildBuildingObjectiveDefinition definition)
        {
            Name = definition.Name;
            buildingDefinition = definition.BuildingDefinition;
            count = definition.Count;
        }

        public void Process()
        {
            IsFulfilled = false;
        }
    }

    public class BuildBuildingObjectiveDefinition : Objective
    {
        public override string Name => "BuildBuildingObjective";

        public BuildingDefinition BuildingDefinition;

        public int Count;
    }
}