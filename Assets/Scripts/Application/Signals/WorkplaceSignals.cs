using Controllers.Work;
using Models.Work;
using Zenject;

namespace App.Signals
{
    public class WorkplaceSignals
    {
        public WorkplaceSignals(DiContainer container)
        {
            container.DeclareSignal<RegisterWorkplace>();
            container.DeclareSignal<UnregisterWorkplace>();
            container.DeclareSignal<RegisterCropField>();
            container.DeclareSignal<UnregisterCropField>();
        }

        public class RegisterWorkplace
        {
            public IWorkplace Workplace { get; private set; }

            public RegisterWorkplace(IWorkplace workplace)
            {
                Workplace = workplace;
            }
        }

        public class UnregisterWorkplace
        {
            public IWorkplace Workplace { get; private set; }

            public UnregisterWorkplace(IWorkplace workplace)
            {
                Workplace = workplace;
            }
        }

        public class RegisterCropField
        {
            public CropModel CropModel { get; private set; }

            public RegisterCropField(CropModel cropModel)
            {
                CropModel = cropModel;
            }
        }

        public class UnregisterCropField
        {
            public CropModel CropModel { get; private set; }

            public UnregisterCropField(CropModel cropModel)
            {
                CropModel = cropModel;
            }
        }
    }
}