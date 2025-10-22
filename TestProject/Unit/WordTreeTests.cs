//required in order to be able to reference data types (e.g. WordTree) from the main project
using SpellCheckingTool;

namespace TestProject.Unit
{
    [TestClass]
    public class WordTreeTests
    {
        [TestMethod]
        public void CreateNewWordTree_ShouldInitialize()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Assert.IsTrue(alphabet != null);
            WordTree tree = new WordTree(alphabet);
            Assert.IsTrue(tree != null);
        }

        [TestMethod]
        public void ParseValidWord_ShouldCreateWord()
        {
            string rawWord = "test";
            IAlphabet alphabet = new LatinAlphabet();
            Word parsedWord = new Word(alphabet, rawWord);
            Assert.IsTrue(parsedWord != null);
            Assert.AreEqual(parsedWord.ToString(), rawWord.ToString());
        }

        [TestMethod]
        public void ParseInvalidWord_ShouldFail()
        {
            string rawWord = "test\n";
            IAlphabet alphabet = new LatinAlphabet();
            Assert.ThrowsException<Exception>(() => { Word parsedWord = new Word(alphabet, rawWord); }, "Word contains invalid characters");
        }

        //TODO: Add tests for adding/searching/removing from the tree
    }
}
