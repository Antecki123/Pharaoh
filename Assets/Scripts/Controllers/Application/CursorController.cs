using App.Helpers;
using App.Signals;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Controllers.Application
{
    public class CursorController : IInitializable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;

        private readonly Dictionary<CursorState, Texture2D> cursorCache = new Dictionary<CursorState, Texture2D>();

        private CursorState currentCursor;

        public CursorController(SignalBus signalBus, PrefabManager prefabManager)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
        }

        public async void Initialize()
        {
            signalBus.Subscribe<ApplicationSignals.SetCursor>(SetCursor);

            await LoadAllCursors();

            ApplyCursor(CursorState.Default);
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<ApplicationSignals.SetCursor>(SetCursor);
        }

        private async UniTask LoadAllCursors()
        {
            foreach (CursorState state in Enum.GetValues(typeof(CursorState)))
            {
                var texture = await prefabManager.LoadObjectAsync<Texture2D>(state.ToString());
                cursorCache[state] = texture;
            }
        }

        private void SetCursor(ApplicationSignals.SetCursor signal)
        {
            ApplyCursor(signal.CursorState);
        }

        private void ApplyCursor(CursorState state)
        {
            if (currentCursor == state)
                return;

            if (!cursorCache.TryGetValue(state, out var texture))
            {
                Debug.LogError($"Cursor {state} not loaded.");
                return;
            }

            currentCursor = state;
            Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
        }
    }

    public enum CursorState
    {
        Default,
        DefaultEnemy,
        DefaultFriends,
        Impossible,
        MiniCannot,
        TargetMoveA,
        TargetMoveB
    }
}