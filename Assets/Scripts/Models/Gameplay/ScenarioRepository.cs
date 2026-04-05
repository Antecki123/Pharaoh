using Controllers.SceneManagment;
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

        public ScenarioData GetNextChapter(SceneName currentChapter, int currentMission)
        {
            ScenarioData nextChapter = null;

            foreach (var scenario in Scenarios)
            {
                if (scenario.Scenario == currentChapter && scenario.Mission == currentMission + 1)
                    return scenario;
            }

            return nextChapter;
        }
    }
}