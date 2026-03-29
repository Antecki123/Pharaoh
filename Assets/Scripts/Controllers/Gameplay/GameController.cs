using Models.Gameplay;
using System;
using System.Collections.Generic;
using Zenject;

namespace Controllers.Gameplay
{
    public class GameController : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus signalBus;

        private List<ScenarioData> scenarioModels;

        private GameState currentState = GameState.MainMenu;
        private ScenarioData currentScenario;

        public GameController(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public void Initialize()
        {

        }

        public void Tick()
        {

        }

        public void Dispose()
        {

        }
    }

    public enum GameState
    {
        MainMenu,
        Game
    }
}