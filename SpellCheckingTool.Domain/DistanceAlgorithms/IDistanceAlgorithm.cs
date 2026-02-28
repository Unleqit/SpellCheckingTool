using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Domain.DistanceAlgorithms;
    public unsafe interface IDistanceAlgorithm
    {
        public abstract int GetDistance(Word a, Word b);
    }
