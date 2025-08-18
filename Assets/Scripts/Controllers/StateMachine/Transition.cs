using System;

namespace Controllers.Ai
{
    public class Transition
    {
        public IState To { get; }
        public Func<bool> Condition { get; }

        public Transition(IState to, Func<bool> condition)
        {
            To = to;
            Condition = condition;
        }
    }
}