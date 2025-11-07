using System;

namespace Models.Economy
{
    public class EconomyModel
    {
        public event Action OnValueChanged;

        public int Settlers { get; private set; }
        public int Gold { get; private set; }

        public void ChangeGold(int value)
        {
            Gold += value;
            OnValueChanged?.Invoke();
        }

        public void ChangeSettlers(int value)
        {
            Settlers += value;
            OnValueChanged?.Invoke();
        }
    }
}