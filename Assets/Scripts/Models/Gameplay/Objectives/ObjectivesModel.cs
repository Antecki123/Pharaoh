using System.Collections.Generic;

namespace Models.Gameplay
{
    public class ObjectivesModel
    {
        public IReadOnlyList<IObjective> Objectives => objectives;

        private List<IObjective> objectives = new List<IObjective>();

        public void AddObjective(IObjective objective)
        {
            objectives.Add(objective);
        }
    }
}