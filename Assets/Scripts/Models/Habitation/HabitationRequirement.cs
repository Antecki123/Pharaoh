using System;

namespace Models.Habitation
{
    public class HabitationRequirement
    {
        public event Action OnValueChanged;

        public float ValuePercent => (CurrentValue / MaxValue) * 100f;
        public float CurrentValue => currentValue;

        public HabitationRequirementDefinition RequirementDefinition => requirementDefinition;
        public int RequiredLevel => requiredLevel;
        public float MaxValue => maxValue;

        private readonly HabitationRequirementDefinition requirementDefinition;
        private readonly int requiredLevel;
        private readonly float maxValue;
        private readonly float decayTime;

        private float currentValue;

        public HabitationRequirement(HabitationRequirementDefinition requirementDefinition, int requiredLevel, float maxValue = 100,
            float decayTime = 0.1f)
        {
            this.requirementDefinition = requirementDefinition;
            this.requiredLevel = requiredLevel;
            this.maxValue = maxValue;
            this.decayTime = decayTime;

            currentValue = maxValue;
        }

        public void Decay(float residentsCount, float deltaTime)
        {
            if (currentValue > 0)
            {
                currentValue -= decayTime * residentsCount * deltaTime;

                if (currentValue < 0)
                    currentValue = 0;

                OnValueChanged?.Invoke();
            }
        }

        public float AddWithResidual(float value)
        {
            var residual = 0f;

            currentValue += value;
            if (currentValue > maxValue)
            {
                residual = currentValue - maxValue;
                currentValue = maxValue;
            }

            OnValueChanged?.Invoke();

            return residual;
        }
    }

    public interface IServiceReceiver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="habitationRequirementDefinition"></param>
        /// <param name="value"></param>
        /// <returns>The method returns the change if the value delivered to the recipient exceeds the maximum value.</returns>
        public float SatisfyResidentNeeds(HabitationRequirementDefinition requirementDefinition, float value);
    }

    public enum HabitationRequirementDefinition
    {
        Water,
        Food,
        Tavern,
        Clothes,
        Pottery,
        Tool,
        Entertainment_1,
        Papyrus,
        Arts,
        Entertainment_2,
        Jewellery,
        Incense,
        Weapon
    }
}