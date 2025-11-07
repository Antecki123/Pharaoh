using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class ImmigrantStrategy : Strategy
    {
        public ImmigrantStrategy(SettlerView settler)
        {
            aiBrain = new AiBrain();
        }
    }
}