using System.Collections.Generic;

namespace Models.Gameplay
{
    public class ScenarioRepository
    {
        public IReadOnlyList<ScenarioData> Scenarios { get; }

        public ScenarioRepository(List<ScenarioData> scenarios)
        {
            Scenarios = scenarios;
        }
    }
}