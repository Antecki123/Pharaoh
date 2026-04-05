using App.Signals;
using Models.Economy;
using Models.Gameplay;
using Models.Helpers;
using System;
using UnityEngine;
using Zenject;

namespace Controllers.Gameplay
{
    public class ScenarioController : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly ObjectivesModel objectivesModel;
        private readonly EconomyModel economyModel;
        private readonly ScenarioModel scenarioModel;
        private readonly ScenarioRepository scenarioRepository;

        private Timer objectivesCheckTimer = new Timer(5f);

        public ScenarioController(SignalBus signalBus, ObjectivesModel objectivesModel, EconomyModel economyModel,
            ScenarioModel scenarioModel, ScenarioRepository scenarioRepository)
        {
            this.signalBus = signalBus;
            this.objectivesModel = objectivesModel;
            this.economyModel = economyModel;
            this.scenarioModel = scenarioModel;
            this.scenarioRepository = scenarioRepository;
        }

        public void Initialize()
        {
            signalBus.Subscribe<GameControlSignals.GameSpeed>(s => SetGameSpeed(s.Speed));
            signalBus.Subscribe<GameControlSignals.ScenarioStarted>(ScenarioStarted);

            economyModel.AddCurrency(scenarioModel.Scenario.BaseGold);
            SetGameSpeed(0f);
        }

        public void Tick()
        {
            // DEBUG
            if (Input.GetKeyUp(KeyCode.L))
                MissionCompleted();

            if (Input.GetKeyUp(KeyCode.K))
                MissionFailed();
            // DEBUG

            objectivesCheckTimer.Tick(Time.deltaTime);

            if (objectivesCheckTimer.IsFinished && objectivesModel.Objectives.Count > 0)
            {
                objectivesCheckTimer.Reset();
                foreach (var objective in objectivesModel.Objectives)
                {
                    if (!objective.IsFulfilled)
                        continue;

                    MissionCompleted();
                }
            }
        }

        public void Dispose()
        {
            signalBus.TryUnsubscribe<GameControlSignals.GameSpeed>(s => SetGameSpeed(s.Speed));
            signalBus.TryUnsubscribe<GameControlSignals.ScenarioStarted>(ScenarioStarted);
        }

        private void SetGameSpeed(float speed)
        {
            Time.timeScale = speed;
            Time.fixedDeltaTime = 0.02f * speed;
        }

        private void ScenarioStarted(GameControlSignals.ScenarioStarted signal)
        {
            SetGameSpeed(1f);
        }

        private void MissionCompleted()
        {
            var nextChapter = scenarioRepository.GetNextChapter(scenarioModel.Scenario.Scenario, scenarioModel.Scenario.Mission);
            if (nextChapter != null)
            {
                scenarioModel.SetupScenario(nextChapter);
                signalBus.Fire(new GameControlSignals.MissionCompleted());
            }
            else
            {
                ScenarioFinished();
            }
        }

        private void MissionFailed()
        {
            signalBus.Fire(new GameControlSignals.MissionFailed());
        }

        private void ScenarioFinished()
        {
            SetGameSpeed(1f);
            signalBus.Fire(new GameControlSignals.ScenarioFinished());
        }
    }
}