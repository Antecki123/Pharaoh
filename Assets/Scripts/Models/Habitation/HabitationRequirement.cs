using System;
using UnityEngine;

namespace Models.Habitation
{
    public class HabitationRequirement
    {
        public event Action OnValueChanged;

        public HabitationRequirementDefinition RequirementDefinition { get; private set; }

        public int Level { get; private set; }

        public float Value { get; private set; } = 100f;

        public float DecayTime { get; private set; } = 0.1f;

        public HabitationRequirement(HabitationRequirementDefinition requirementDefinition, int level)
        {
            RequirementDefinition = requirementDefinition;
            Level = level;
        }

        public void Decay(float residentsCount)
        {
            if (Value > 0)
            {
                Value -= DecayTime * residentsCount * Time.deltaTime;
                OnValueChanged?.Invoke();

                if (Value < 0)
                    Value = 0;
            }
        }
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