using SpellCheckingTool;

namespace TestProject.Unit
{
    [TestClass]
    public unsafe class SuggestionTests
    {
#pragma warning disable CS8618 // tree is assigned in SetupTests() before the tests of this class get executed
        WordTree tree;
#pragma warning restore CS8618 

        [TestInitialize]
        public void SetupTests()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"../../../.."));
            string path = Path.Combine(projectRoot, @"TestProject/Resources/wordFile.wdb");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Wörterbuchdatei nicht gefunden: {path}");

            var filePath = new FilePath(path);
            tree = new WordTree();
            tree = new FilePersistenceService(tree).Load(filePath);
        }

        [TestMethod]
        public void GetHundredSuggestionsOfAllDistancesForTreeWithTenElements_ShouldReturnTenSuggestions()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word[] words = Word.ParseWords(alphabet, new string[] { "these", "are", "some", "random", "english", "words", "containing", "only", "latin", "characters" });
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(words);
            SuggestionResult result = tree.GetSuggestions("containing", 100, 999);
            Assert.AreEqual(result.GetSuggestionCount(), tree.metaData.wordCount);
            tree.Dispose();
        }

        [TestMethod]
        public void GetFiveSuggestionsInLargeTreeForContain_ShouldReturnContainContains()
        {
            SuggestionResult result = tree.GetSuggestions("contain", 5, 999);
            Assert.AreEqual(result.GetSuggestionCount(), 5);
            Assert.IsTrue(result.GetSuggestionArrayManaged().SequenceEqual(new string[] { "contain", "contains", "congaing", "conin", "conjoin" }));
        }

        [TestMethod]
        public void GetSuggestionsTwice_ShouldYieldSameResult()
        {
            SuggestionResult result = tree.GetSuggestions("aba", 50, 999);
            Assert.AreEqual(result.GetSuggestionCount(), 50);
            SuggestionResult result2 = tree.GetSuggestions("aba", 50, 999);
            Assert.AreEqual(result2.GetSuggestionCount(), 50);
            Assert.IsTrue(result.GetSuggestionArrayManaged().SequenceEqual(result2.GetSuggestionArrayManaged()));
        }

        [TestMethod]
        public void GetSuggestionsOfWordNotContainedInTreeWithMaxDistanceParameterZero_ShouldReturnArrayContainingOnlyNullValues()
        {
            SuggestionResult result = tree.GetSuggestions("contai", 50, 0);
            Assert.IsTrue(result.GetSuggestionCount() == 0);
            Assert.IsTrue(result.GetSuggestionArrayManaged().All(element => element == null));
        }

        [TestMethod]
        public void GetSuggestionForWordContainingCharactersNotPresentInWordTreeAlphabetWithMaxDistance_ShouldReturnFirst5AlphabeticallyOrderedWordsOfWordTree()
        {
            SuggestionResult result = tree.GetSuggestions("تزنيت", 5, 999);
            Assert.IsTrue(result.GetSuggestionCount() == 5);
            Assert.IsTrue(result.GetSuggestionArrayManaged().SequenceEqual(new string[] { "aah", "aalii", "aal", "aargh", "aa" } ));
        }

        [TestMethod]
        public void GetSuggestionsOfWordContainedInTreeWithMaxDistanceParameterZero_ShouldReturnArrayContainingOnlyExactMatchAndNullValues()
        {
            SuggestionResult result = tree.GetSuggestions("contain", 50, 0);
            Assert.IsTrue(result.GetSuggestionCount() == 1);
            Assert.IsTrue(result.GetSuggestionArrayManaged()[0] == "contain");
            Assert.IsTrue(result.GetSuggestionArrayManaged().Take(new Range(1, 50)).All(element => element == null));
        }
    }
}
