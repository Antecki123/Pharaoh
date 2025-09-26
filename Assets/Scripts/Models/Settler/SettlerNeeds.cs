using System;

namespace Models.Settler
{
    public class SettlerNeeds
    {
        public event Action OnValueChanged;

        public float Health { get; private set; }

        public float Hunger { get; private set; }

        public float Entertainment { get; private set; }

        public float Sleep { get; private set; }

        public float Morale { get; private set; }

        public void SetHealth(float value)
        {
            Health = value;
            OnValueChanged?.Invoke();
        }

        public void SetHunger(float value)
        {
            Hunger = value;
            OnValueChanged?.Invoke();
        }

        public void SetEntertainment(float value)
        {
            Entertainment = value;
            OnValueChanged?.Invoke();
        }

        public void SetSleep(float value)
        {
            Sleep = value;
            OnValueChanged?.Invoke();
        }

        public void SetMorale(float value)
        {
            Morale = value;
            OnValueChanged?.Invoke();
        }
    }
}