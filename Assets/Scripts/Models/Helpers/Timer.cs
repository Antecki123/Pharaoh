using System;

namespace Models.Helpers
{
    public class Timer
    {
        public event Action OnCompleted;

        public bool IsFinished => elapsed >= duration;
        public float Elapsed => elapsed;

        private float duration;
        private float elapsed;

        public Timer(float duration)
        {
            this.duration = duration;
        }

        public void Reset()
        {
            elapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            elapsed += deltaTime;

            if (elapsed >= duration)
                OnCompleted?.Invoke();
        }
    }
}