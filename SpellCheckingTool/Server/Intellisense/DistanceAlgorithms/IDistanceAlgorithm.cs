namespace SpellCheckingTool
{
    public unsafe interface IDistanceAlgorithm
    {
        public abstract int GetDistance(Word a, Word b);
    }
}
