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
            LevenshteinDistanceAlgorithm distanceAlgorithm = new LevenshteinDistanceAlgorithm(tree);
            Assert.AreEqual(distanceAlgorithm.BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(distanceAlgorithm.BufferSize, word2.Length + 1);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchSameWord_ShouldReturnZero()
        {
            string word = "test";
            string word2 = "test";
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(new Word(alphabet, word));
            LevenshteinDistanceAlgorithm distanceAlgorithm = new LevenshteinDistanceAlgorithm(tree);
            Assert.AreEqual(distanceAlgorithm.BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(distanceAlgorithm.BufferSize, word2.Length + 1);
            int distance = distanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 0);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchTestAndText_ShouldReturnOne()
        {
            string word = "test";
            string word2 = "text";
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(new Word(alphabet, word));
            LevenshteinDistanceAlgorithm distanceAlgorithm = new LevenshteinDistanceAlgorithm(tree);
            Assert.AreEqual(distanceAlgorithm.BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(distanceAlgorithm.BufferSize, word2.Length + 1);
            int distance = distanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 1);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchInterestingAndImplementation_ShouldReturnNine()
        {
            string word = "interesting";
            string word2 = "implementation";
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(new Word(alphabet, word));
            LevenshteinDistanceAlgorithm distanceAlgorithm = new LevenshteinDistanceAlgorithm(tree);
            Assert.AreEqual(distanceAlgorithm.BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(distanceAlgorithm.BufferSize, word2.Length + 1);
            int distance = distanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 9);
        }
    }
}
