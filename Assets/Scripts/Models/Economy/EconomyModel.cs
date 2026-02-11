using System;

namespace Models.Economy
{
    public class EconomyModel
    {
        public event Action OnValueChanged;

        public int Settlers { get; private set; } = 0;
        public int Currency { get; private set; } = 1000;

        public void AddCurrency(int value)
        {
            Currency += value;
            OnValueChanged?.Invoke();
        }

        public void RemoveCurrency(int value)
        {
            Currency -= value;
            OnValueChanged?.Invoke();
        }

        public void AddSettlers(int value)
        {
            Settlers += value;
            OnValueChanged?.Invoke();
        }

        public void RemoveSettlers(int value)
        {
            Settlers -= value;
            OnValueChanged?.Invoke();
        }
    }
}