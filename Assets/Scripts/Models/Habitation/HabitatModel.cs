using Controllers.Work;
using Models.Economy;
using Models.Helpers;
using Models.Settler;
using System;
using System.Collections.Generic;

namespace Models.Habitation
{
    public class HabitatModel
    {
        public event Action OnValueChanged;

        public string Name => namesPerLevel[currentLevel - 1];
        public int MaxResidents => residentsPerLevel[currentLevel - 1];
        public int CurrentLevel => currentLevel;
        public int MinLevel => minLevel;
        public int MaxLevel => maxLevel;

        public LevelChangeState LevelChangeState => levelChangeState;

        public IReadOnlyList<SettlerModel> Residents => residents;
        public IReadOnlyList<CommodityModel> Storage => storage;
        public IReadOnlyDictionary<HabitatRequirementDefinition, HabitationRequirement> HabitationRequirements => habitationRequirements;
        public IReadOnlyDictionary<Type, float> MunicipalServices => municipalServices;

        private readonly List<SettlerModel> residents = new List<SettlerModel>();
        private readonly List<CommodityModel> storage = new List<CommodityModel>();
        private readonly Dictionary<HabitatRequirementDefinition, HabitationRequirement> habitationRequirements;
        private readonly Dictionary<Type, float> municipalServices;

        private int currentLevel = 1;

        private readonly int[] residentsPerLevel;
        private readonly string[] namesPerLevel;

        private readonly Timer upgradeTimer = new Timer(5f);
        private readonly Timer downgradeTimer = new Timer(5f);
        private readonly Timer levelChangeTimer = new Timer(5f);

        private LevelChangeState levelChangeState;

        private readonly int minLevel = 1;
        private readonly int maxLevel = 3;

        public HabitatModel(string[] namesPerLevel, int[] residentsPerLevel)
        {
            this.namesPerLevel = namesPerLevel;
            this.residentsPerLevel = residentsPerLevel;

            habitationRequirements = new Dictionary<HabitatRequirementDefinition, HabitationRequirement>()
            {
                { HabitatRequirementDefinition.Water, new(HabitatRequirementDefinition.Water, 1, 10, 0.05f) },
                { HabitatRequirementDefinition.Food, new(HabitatRequirementDefinition.Food, 1, 50) },
                { HabitatRequirementDefinition.Tavern, new(HabitatRequirementDefinition.Tavern, 1) },
                { HabitatRequirementDefinition.Clothes, new(HabitatRequirementDefinition.Clothes, 2, 50) },
                { HabitatRequirementDefinition.Pottery, new(HabitatRequirementDefinition.Pottery, 2, 30) },
                { HabitatRequirementDefinition.Tool, new(HabitatRequirementDefinition.Tool, 2, 20) },
                { HabitatRequirementDefinition.Entertainment, new(HabitatRequirementDefinition.Entertainment, 2) },
                { HabitatRequirementDefinition.Arts, new(HabitatRequirementDefinition.Arts, 3) },
                { HabitatRequirementDefinition.Papyrus, new(HabitatRequirementDefinition.Papyrus, 3, 50) }
            };

            municipalServices = new()
            {
                { typeof(TaxCollectionService), 1f },
                { typeof(FireProtectionService), 1f },
                { typeof(ReligionService), 1f },
            };
        }

        public void AddResident(SettlerModel settler)
        {
            residents.Add(settler);
            OnValueChanged?.Invoke();
        }

        public void RemoveResident(SettlerModel settler)
        {
            residents.Remove(settler);
            OnValueChanged?.Invoke();
        }

        public void AddCommodity(CommodityModel commodity)
        {
            storage.Add(commodity);
            OnValueChanged?.Invoke();
        }

        public void RemoveCommodity(CommodityModel commodity)
        {
            storage.Remove(commodity);
            OnValueChanged?.Invoke();
        }

        public bool HasAvailableSpot()
        {
            return residentsPerLevel[currentLevel - 1] - residents.Count > 0;
        }

        public void SetUpgradeTimer(float deltaTime, bool resetToken = false)
        {
            if (resetToken)
            {
                upgradeTimer.Reset();
                return;
            }

            upgradeTimer.Tick(deltaTime);

            if (upgradeTimer.IsFinished)
            {
                upgradeTimer.Reset();
                levelChangeState = LevelChangeState.Upgrading;

                OnValueChanged?.Invoke();
            }
        }

        public void SetDowngradeTimer(float deltaTime, bool resetToken = false)
        {
            if (resetToken)
            {
                downgradeTimer.Reset();
                return;
            }

            downgradeTimer.Tick(deltaTime);

            if (downgradeTimer.IsFinished)
            {
                levelChangeState = LevelChangeState.Downgrading;
                downgradeTimer.Reset();
                OnValueChanged?.Invoke();
            }
        }

        public void LevelChange(float deltaTime)
        {
            levelChangeTimer.Tick(deltaTime);

            if (levelChangeTimer.IsFinished)
            {
                if (levelChangeState == LevelChangeState.Upgrading)
                    currentLevel++;
                else if (levelChangeState == LevelChangeState.Downgrading)
                    currentLevel--;

                currentLevel = Math.Clamp(currentLevel, minLevel, maxLevel);

                levelChangeState = LevelChangeState.None;
                levelChangeTimer.Reset();
                OnValueChanged?.Invoke();
            }
        }

        public void SatisfyResidentNeeds(HabitatRequirementDefinition requirementDefinition)
        {
            if (habitationRequirements.TryGetValue(requirementDefinition, out var requirement))
                requirement?.SatisfyNeed();
        }

        public void ReceiveService(IService service)
        {
            switch (service)
            {
                case TaxCollectionService tax:
                    municipalServices[tax.GetType()] = tax.Value;
                    break;

                case ReligionService religion:
                    municipalServices[religion.GetType()] = religion.Value;
                    break;

                case FireProtectionService fireProtection:
                    municipalServices[fireProtection.GetType()] = fireProtection.Value;
                    break;

                case HabitationRequirementService habitation:
                    SatisfyResidentNeeds(habitation.RequirementDefinition);
                    break;
            }
        }
    }
}
