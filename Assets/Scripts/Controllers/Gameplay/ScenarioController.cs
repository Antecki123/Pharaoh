using Models.Gameplay;
using Models.Helpers;
using UnityEngine;
using Zenject;

namespace Controllers.Gameplay
{
    public class ScenarioController : IInitializable, ITickable
    {
        private readonly ObjectivesModel objectivesModel;

        private Timer objectivesCheckTimer = new Timer(5f);

        public ScenarioController(ObjectivesModel objectivesModel)
        {
            this.objectivesModel = objectivesModel;
        }

        public void Initialize()
        {

        }

        public void Tick()
        {
            objectivesCheckTimer.Tick(Time.deltaTime);

            if (objectivesCheckTimer.IsFinished && objectivesModel.Objectives.Count > 0)
            {
                objectivesCheckTimer.Reset();
                foreach (var objective in objectivesModel.Objectives)
                {
                    if (!objective.IsFulfilled)
                        continue;

                    ScenarioCompleted();
                }
            }
        }

        private void ScenarioCompleted()
        {

        }

        private void ScenarioFailed()
        {

        }
    }
}