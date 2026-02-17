namespace SpellCheckingTool
{
    public unsafe interface IDistanceAlgorithm
    {
        public abstract int GetDistance(string a, string b);
    }
}
