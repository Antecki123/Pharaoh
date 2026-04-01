using System;

namespace Models.Gameplay
{
    [Serializable]
    public abstract class Objective
    {
        public abstract string Name { get; }
    }

    public interface IObjective
    {
        public string Name { get; }

        public bool IsFulfilled { get; }

        public abstract void Process();

        public string ProgressDisplay { get; }
    }
}