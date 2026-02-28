using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.DistanceAlgorithms;

namespace SpellCheckingTool.Domain.WordTree;
    public class WordTreeParameters
    {
        public IAlphabet? alphabet { get; set; }
        //public IPersistenceService? persistenceService { get; set; }
        public IDistanceAlgorithm? distanceAlgorithm { get; set; }
    }
