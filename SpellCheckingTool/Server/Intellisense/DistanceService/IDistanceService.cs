namespace SpellCheckingTool
{
    public unsafe interface IDistanceService
    {
        public abstract int GetDistance(Word a, Word b);
    }
}
