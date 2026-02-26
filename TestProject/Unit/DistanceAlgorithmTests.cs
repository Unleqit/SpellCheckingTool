using SpellCheckingTool;

namespace TestProject.Unit
{
    [TestClass]
    public class DistanceAlgorithmTests
    {
        [TestMethod]
        public void LevenshteinDistanceMatchingResizeWordTree_ShouldResizeInternalBuffers()
        {
            string word = "example";
            string word2 = "longerExample";
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(new Word(alphabet, word));
            LevenshteinDistanceService distanceAlgorithm = new LevenshteinDistanceService(tree);
            tree.Add(new Word(alphabet, word2));
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchSameWord_ShouldReturnZero()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word word = new Word(alphabet, "test");
            Word word2 = new Word(alphabet, "test");
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(word);
            LevenshteinDistanceService distanceAlgorithm = new LevenshteinDistanceService(tree);
            tree.Add(word2);
            int distance = distanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 0);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchTestAndText_ShouldReturnOne()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word word = new Word(alphabet, "test");
            Word word2 = new Word(alphabet, "text");
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(word);
            LevenshteinDistanceService distanceAlgorithm = new LevenshteinDistanceService(tree);
            tree.Add(word2);
            int distance = distanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 1);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchInterestingAndImplementation_ShouldReturnNine()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word word = new Word(alphabet, "interesting");
            Word word2 = new Word(alphabet, "implementation");
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(word);
            LevenshteinDistanceService distanceAlgorithm = new LevenshteinDistanceService(tree);
            tree.Add(word2);
            int distance = distanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 9);
        } 
    }
}
