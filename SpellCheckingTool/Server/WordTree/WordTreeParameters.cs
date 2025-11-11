namespace SpellCheckingTool
{
    public class WordTreeParameters
    {
        public IAlphabet? alphabet { get; set; }
        public IPersistenceService? persistenceService { get; set; }
        public IDistanceAlgorithm? distanceAlgorithm { get; set; }
    }
}
