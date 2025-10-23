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

        [TestMethod]
        public void ParseMultipleWords_ShouldSucceed()
        {
            string[] rawWords = new string[] { "test", "anotherWord" };
            IAlphabet alphabet = new LatinAlphabet();
            Word[] words = Word.ParseWords(alphabet, rawWords);
            Assert.AreEqual(words.Length, rawWords.Length);
        }

        [TestMethod]
        public void ParseMultipleWordsIncludingInvalidWords_ShouldOnlyReturnArrayOfValidWords()
        {
            string[] rawWords = new string[] { "test", "anotherWord", "ungültigesWort" };
            IAlphabet alphabet = new LatinAlphabet();
            Word[] words = Word.ParseWords(alphabet, rawWords);
            Assert.AreEqual(words.Length, rawWords.Length - 1);
        }

        //TODO: Add tests for adding/searching/removing from the tree
        [TestMethod]
        public void AddWordToTree_ShouldReturnSuccessCount()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            int successCount = tree.Add(new Word(alphabet, "test"));
            Assert.AreEqual(successCount, 1);
        }

        [TestMethod]
        public void AddWordAlreadyContainedInTree_ShouldNotAddWord()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            int successCount = tree.Add(new Word(alphabet, "test"));
            Assert.AreEqual(successCount, 1);
            int successCount2 = tree.Add(new Word(alphabet, "test"));
            Assert.AreEqual(successCount2, 0);
        }

        [TestMethod]
        public void SearchWordContainedInTree_ShouldReturnTrue()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            Word w = new Word(alphabet, "test");
            int successCount = tree.Add(w);
            Assert.AreEqual(successCount, 1);
            bool result = tree.Contains(w);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void SearchWordNotContainedInTree_ShouldReturnFalse()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            Word w = new Word(alphabet, "test");
            int successCount = tree.Add(w);
            Assert.AreEqual(successCount, 1);
            bool result = tree.Contains(new Word(alphabet, "someOtherWord"));
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void RemoveWordContainedInTree_ShouldRemoveWordAndReturnSuccess()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            Word w = new Word(alphabet, "test");
            int successCount = tree.Add(w);
            Assert.AreEqual(successCount, 1);
            int removeSuccessCount = tree.Remove(w);
            Assert.AreEqual(removeSuccessCount, 1);
        }

        [TestMethod]
        public void RemoveWordNotContainedInTree_ShouldReturnZero()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            Word w = new Word(alphabet, "test");
            int successCount = tree.Add(w);
            Assert.AreEqual(successCount, 1);
            int removeSuccessCount = tree.Remove(new Word(alphabet, "someOtherWord"));
            Assert.AreEqual(removeSuccessCount, 0);
        }

        [TestMethod]
        public void RemoveShorterWord_ShouldKeepLongerWordAndOnlyRemoveShorterOne()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            Word w = new Word(alphabet, "lol");
            Word w2 = new Word(alphabet, "lollipop");
            int successCount = tree.Add(new Word[] { w, w2 });
            Assert.AreEqual(successCount, 2);
            int removeSuccessCount = tree.Remove(w);
            Assert.AreEqual(removeSuccessCount, 1);
            bool containsRemovedWord = tree.Contains(w);
            Assert.AreEqual(containsRemovedWord, false);
            bool containsLongerWord = tree.Contains(w2);
            Assert.AreEqual(containsLongerWord, true);
        }

        [TestMethod]
        public void RemoveLongerWord_ShouldKeepShorterWordAndOnlyRemoveLongerOne()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet);
            Word w = new Word(alphabet, "lol");
            Word w2 = new Word(alphabet, "lollipop");
            int successCount = tree.Add(new Word[] { w, w2 });
            Assert.AreEqual(successCount, 2);
            int removeSuccessCount = tree.Remove(w2);
            Assert.AreEqual(removeSuccessCount, 1);
            bool containsRemovedWord = tree.Contains(w2);
            Assert.AreEqual(containsRemovedWord, false);
            bool containsShorterWord = tree.Contains(w);
            Assert.AreEqual(containsShorterWord, true);
        }

    }
}
