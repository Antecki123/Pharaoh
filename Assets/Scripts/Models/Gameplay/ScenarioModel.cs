namespace Models.Gameplay
{
    public class ScenarioModel
    {
        public bool IsScenarioLoaded => scenario != null;

        public ScenarioData Scenario => scenario;

        private ScenarioData scenario;

        public void SetupScenario(ScenarioData scenario)
        {
            this.scenario = scenario;
        }
    }
}