using Zenject;

namespace App.Signals
{
    public class SignalInstaller
    {
        public SignalInstaller(DiContainer container)
        {
            new ApplicationSignals(container);
            new ConstructionSignals(container);
        }
    }
}