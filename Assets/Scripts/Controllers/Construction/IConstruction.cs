namespace Controllers.Construction
{
    public interface IConstruction
    {
        public bool RoadConnectionRequired { get; }

        public void Tick();
    }
}