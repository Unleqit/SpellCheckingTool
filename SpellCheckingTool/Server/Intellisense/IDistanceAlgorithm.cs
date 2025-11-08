namespace SpellCheckingTool
{
    public unsafe interface IDistanceAlgorithm
    {
        public abstract int GetDistance(string a, string b);

        public abstract int GetDistance(char* a, char* b, int aLength, int bLength);
    }
}
