using System;

namespace Models.Habitation
{
    public class HabitationRequirement
    {
        public event Action OnValueChanged;

        public float ValuePercent => (CurrentValue / MaxValue) * 100f;
        public float CurrentValue => currentValue;

        public HabitatRequirementDefinition RequirementDefinition => requirementDefinition;
        public int RequiredLevel => requiredLevel;
        public float MaxValue => maxValue;

        private readonly HabitatRequirementDefinition requirementDefinition;
        private readonly int requiredLevel;
        private readonly float maxValue;
        private readonly float decayTime;

        private float currentValue;

        public HabitationRequirement(HabitatRequirementDefinition requirementDefinition, int requiredLevel,
            float maxValue = 100f, float decayTime = 0.1f)
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

        public void SatisfyNeed()
        {
            currentValue = maxValue;
        }
    }

    public enum HabitatRequirementDefinition
    {
        Water,
        Food,
        Tavern,
        Clothes,
        Pottery,
        Tool,
        Entertainment,
        Papyrus,
        Arts,
        Jewellery,
        Incense,
        Weapon
    }
}