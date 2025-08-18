using System;
using System.Collections.Generic;

namespace Controllers.Ai
{
    public class AiBrain
    {
        private Dictionary<Type, HashSet<Transition>> transitions = new Dictionary<Type, HashSet<Transition>>();
        private HashSet<Transition> currentTransitions = new HashSet<Transition>();
        private HashSet<Transition> anyTransitions = new HashSet<Transition>();
        private static HashSet<Transition> EmptyTransitions = new HashSet<Transition>(0);

        private IState currentState;

        public void UpdateAction()
        {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            currentState?.Tick();
        }

        public void SetState(IState state)
        {
            if (currentState == state) return;

            currentState?.OnExit();
            currentState = state;

            transitions.TryGetValue(currentState.GetType(), out currentTransitions);
            currentTransitions ??= EmptyTransitions;

            currentState.OnEnter();
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (transitions.TryGetValue(from.GetType(), out var _transitions) == false)
            {
                _transitions = new HashSet<Transition>();
                transitions[from.GetType()] = _transitions;
            }
            _transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState state, Func<bool> predicate)
        {
            anyTransitions.Add(new Transition(state, predicate));
        }

        private Transition GetTransition()
        {
            foreach (var transition in anyTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }
    }
}