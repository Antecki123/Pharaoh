using Models.Habitation;

namespace Controllers.Work
{
    public interface IService { }

    public interface IServiceReceiver
    {
        void ReceiveService(IService service);
    }

    public struct TaxCollectionService : IService {
        public float Value { get; private set; }

        public TaxCollectionService(float value)
        {
            Value = value;
        }
    }

    public struct ReligionService : IService
    {
        public float Value { get; private set; }

        public ReligionService(float value)
        {
            Value = value;
        }
    }

    public struct HabitationRequirementService : IService
    {
        public HabitatRequirementDefinition RequirementDefinition { get; private set; }
        public float Value { get; private set; }

        public HabitationRequirementService(HabitatRequirementDefinition requirementDefinition, float value)
        {
            RequirementDefinition = requirementDefinition;
            Value = value;
        }
    }

    public struct FireProtectionService : IService
    {
        public float Value { get; private set; }

        public FireProtectionService(float value)
        {
            Value = value;
        }
    }

    public enum ServiceType
    {
        TaxCollectionService,
        ReligionService,
        HabitationRequirementService,
        FireProtectionService
    }
}


/*public void ReceiveService(IService service)
{
    switch (service)
    {
        case FireProtectionService fireProtection:
            municipalServices[fireProtection.GetType()] = fireProtection.Value;
            break;
    }
}*/