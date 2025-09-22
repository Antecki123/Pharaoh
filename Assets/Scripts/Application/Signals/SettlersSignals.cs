using UnityEngine;
using Views.Settler;
using Zenject;

namespace App.Signals
{
    public class SettlersSignals
    {
        public SettlersSignals(DiContainer container)
        {
            container.DeclareSignal<SpawnSettler>();
            container.DeclareSignal<DespawnSettler>();
        }

        public class SpawnSettler
        {
            public Vector3 Position { get; private set; }
            public Quaternion Rotation { get; private set; }

            public SpawnSettler(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }

        public class DespawnSettler
        {
            public SettlerView SettlerView { get; private set; }

            public DespawnSettler(SettlerView settlerView)
            {
                SettlerView = settlerView;
            }
        }
    }
}