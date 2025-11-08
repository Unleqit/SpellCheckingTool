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
            WordTree tree = new WordTree(alphabet);

            tree.Add(new Word(alphabet, word));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word2.Length + 1);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchSameWord_ShouldReturnZero()
        {
            string word = "test";
            string word2 = "test";
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet); //TODO: mock this tree stuff away, as its not really needed for this
            tree.Add(new Word(alphabet, word));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word2.Length + 1);
            int distance = tree.DistanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 0);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchTestAndText_ShouldReturnOne()
        {
            string word = "test";
            string word2 = "text";
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet); //TODO: mock this tree stuff away, as its not really needed for this
            tree.Add(new Word(alphabet, word));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word2.Length + 1);
            int distance = tree.DistanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 1);
        }

        [TestMethod]
        public void LevenshteinDistanceMatchingMatchInterestingAndImplementation_ShouldReturnNine()
        {
            string word = "interesting";
            string word2 = "implementation";
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet); //TODO: mock this tree stuff away, as its not really needed for this
            tree.Add(new Word(alphabet, word));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word.Length + 1);
            tree.Add(new Word(alphabet, word2));
            Assert.AreEqual(((LevenshteinDistanceAlgorithm)tree.DistanceAlgorithm).BufferSize, word2.Length + 1);
            int distance = tree.DistanceAlgorithm.GetDistance(word, word2);
            Assert.AreEqual(distance, 9);
        }
    }
}
