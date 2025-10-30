//required in order to be able to reference data types (e.g. WordTree) from the main project
using SpellCheckingTool;
using System.Diagnostics;

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
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
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
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            int successCount = tree.Add(new Word(alphabet, "test"));
            Assert.AreEqual(successCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 1);
        }

        [TestMethod]
        public void AddWordAlreadyContainedInTree_ShouldNotAddWord()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            int successCount = tree.Add(new Word(alphabet, "test"));
            Assert.AreEqual(successCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 1);
            int successCount2 = tree.Add(new Word(alphabet, "test"));
            Assert.AreEqual(successCount2, 0);
            Assert.AreEqual(tree.metaData.wordCount, 1);
        }

        [TestMethod]
        public void SearchWordContainedInTree_ShouldReturnTrue()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            Word w = new Word(alphabet, "test");
            int successCount = tree.Add(w);
            Assert.AreEqual(successCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 1);
            bool result = tree.Contains(w);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void SearchWordNotContainedInTree_ShouldReturnFalse()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
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
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            Word w = new Word(alphabet, "test");
            int successCount = tree.Add(w);
            Assert.AreEqual(successCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 1);
            int removeSuccessCount = tree.Remove(w);
            Assert.AreEqual(removeSuccessCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 0);
        }

        [TestMethod]
        public void RemoveWordNotContainedInTree_ShouldReturnZero()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            Word w = new Word(alphabet, "test");
            int successCount = tree.Add(w);
            Assert.AreEqual(successCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 1);
            int removeSuccessCount = tree.Remove(new Word(alphabet, "someOtherWord"));
            Assert.AreEqual(removeSuccessCount, 0);
            Assert.AreEqual(tree.metaData.wordCount, 1);
        }

        [TestMethod]
        public void RemoveShorterWord_ShouldKeepLongerWordAndOnlyRemoveShorterOne()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            Word w = new Word(alphabet, "lol");
            Word w2 = new Word(alphabet, "lollipop");
            int successCount = tree.Add(new Word[] { w, w2 });
            Assert.AreEqual(successCount, 2);
            Assert.AreEqual(tree.metaData.wordCount, 2);
            int removeSuccessCount = tree.Remove(w);
            Assert.AreEqual(removeSuccessCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 1);
            bool containsRemovedWord = tree.Contains(w);
            Assert.AreEqual(containsRemovedWord, false);
            bool containsLongerWord = tree.Contains(w2);
            Assert.AreEqual(containsLongerWord, true);
        }

        [TestMethod]
        public void RemoveLongerWord_ShouldKeepShorterWordAndOnlyRemoveLongerOne()
        {
            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            Word w = new Word(alphabet, "lol");
            Word w2 = new Word(alphabet, "lollipop");
            int successCount = tree.Add(new Word[] { w, w2 });
            Assert.AreEqual(successCount, 2);
            Assert.AreEqual(tree.metaData.wordCount, 2);
            int removeSuccessCount = tree.Remove(w2);
            Assert.AreEqual(removeSuccessCount, 1);
            Assert.AreEqual(tree.metaData.wordCount, 1);
            bool containsRemovedWord = tree.Contains(w2);
            Assert.AreEqual(containsRemovedWord, false);
            bool containsShorterWord = tree.Contains(w);
            Assert.AreEqual(containsShorterWord, true);
        }

        [TestMethod]
        public void ParseProductionSizedWordTreeFromStringArray_ShouldTakeLessThanTwoSecondsToComplete()
        {
            string resourceDirectory = Directory.GetCurrentDirectory(); //prints .../SpellCheckingTool/TestProject/bin/Debug/net8.0
            resourceDirectory += @"/../../../Resources/";

            string[] rawWords = File.ReadAllText(resourceDirectory + "wordfile.txt").Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(rawWords.Length > 0);

            Stopwatch sw = Stopwatch.StartNew();

            IAlphabet alphabet = new LatinAlphabet();
            Word[] parsedWords = Word.ParseWords(alphabet, rawWords);
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            tree.Add(parsedWords);
            Assert.AreEqual(tree.metaData.wordCount, rawWords.Length);

            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 2000);
        }

        [TestMethod]
        public void DeserializeProductionSizedWordTreeFromWDBFile_ShouldTakeLessThanTwoSecondsToComplete()
        {
            string resourceDirectory = Directory.GetCurrentDirectory(); //prints .../SpellCheckingTool/TestProject/bin/Debug/net8.0
            resourceDirectory += @"/../../../Resources/";
            
            string[] rawWords = File.ReadAllText(resourceDirectory + "wordfile.txt").Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);

            Stopwatch sw = Stopwatch.StartNew();

            IAlphabet alphabet = new LatinAlphabet();
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            tree = tree.Deserialize(new FilePath(resourceDirectory + @"/wordfile.wdb"));
            Assert.AreEqual(tree.metaData.wordCount, rawWords.Length);

            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 2000);
        }

        [TestMethod]
        public void SerializeProductionSizedWordTree_ShouldTakeLessThanTwoSecondsToComplete()
        {
            string resourceDirectory = Directory.GetCurrentDirectory(); //prints .../SpellCheckingTool/TestProject/bin/Debug/net8.0
            resourceDirectory += @"/../../../Resources/";

            string[] rawWords = File.ReadAllText(resourceDirectory + "wordfile.txt").Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(rawWords.Length > 0);

            IAlphabet alphabet = new LatinAlphabet();
            Word[] parsedWords = Word.ParseWords(alphabet, rawWords);
            WordTree tree = new WordTree(alphabet, new FilePersistenceService());
            tree.Add(parsedWords);
            Assert.AreEqual(tree.metaData.wordCount, rawWords.Length);
            
            Stopwatch sw = Stopwatch.StartNew();

            tree.Serialize(new FilePath(Directory.GetCurrentDirectory() + @"/tmp.wdb"));
            Assert.IsTrue(File.Exists(Directory.GetCurrentDirectory() + @"/tmp.wdb"));

            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 2000);

            //clean up temporary files
            File.Delete(Directory.GetCurrentDirectory() + @"/tmp.wdb");
            Assert.IsFalse(File.Exists(Directory.GetCurrentDirectory() + @"/tmp.wdb"));
        }
    }
}
