using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class ImmigrantStrategy : Strategy
    {
        public class Factory : PlaceholderFactory<SettlerView, ImmigrantStrategy> { }

        public ImmigrantStrategy(SettlerView settler)
        {
            aiBrain = new AiBrain();
        }
    }
}