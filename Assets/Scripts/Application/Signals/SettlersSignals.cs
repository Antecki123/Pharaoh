using UnityEngine;
using Zenject;

namespace App.Signals
{
    public class SettlersSignals
    {
        public SettlersSignals(DiContainer container)
        {
            container.DeclareSignal<SpawnSettler>();
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
    }
}