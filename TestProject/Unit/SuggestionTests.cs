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
            string path = Path.Combine(projectRoot, @"TestProject/Resources/wordFile.json");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Wordfile not found at {path}");

            var filePath = new FilePath(path);
            tree = new WordTree();
            tree = new FilePersistenceService(tree).Load(filePath);
        }

        [TestMethod]
        public void GetTwentySuggestionsOfAllDistancesForTreeWithTenElements_ShouldReturnTenSuggestions()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word[] words = Word.ParseWords(alphabet, new string[] { "these", "are", "some", "random", "english", "words", "containing", "only", "latin", "characters" });
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(words);
            SuggestionResult result = tree.GetSuggestions(new Word(new LatinAlphabet(), "containing"), 20, 999);
            Assert.AreEqual(result.GetSuggestionCount(), tree.metaData.wordCount);
        }

        [TestMethod]
        public void GetFiveSuggestionsInLargeTreeForContain_ShouldReturnContainContainsCongaingConinConjoin()
        {
            SuggestionResult result = tree.GetSuggestions(new Word(new LatinAlphabet(), "contain"), 5, 999);
            Assert.AreEqual(result.GetSuggestionCount(), 5);
            Assert.IsTrue(result.GetSuggestionArray().Select((word) => word.ToString()).SequenceEqual(new string[] { "contain", "contains", "congaing", "conin", "conjoin" }));
        }

        [TestMethod]
        public void GetHundredSuggestionsInLargeTreeForContain_ShouldReturnTwentySuggestions()
        {
            SuggestionResult result = tree.GetSuggestions(new Word(new LatinAlphabet(), "contain"), 20, 999);
            Assert.AreEqual(result.GetSuggestionCount(), 20);
            Assert.IsTrue(result.GetSuggestionArray()[0].ToString() == "contain");
        }

        [TestMethod]
        public void GetSuggestionsTwice_ShouldYieldSameResult()
        {
            SuggestionResult result = tree.GetSuggestions(new Word(new LatinAlphabet(), "aba"), 20, 999);
            Assert.AreEqual(result.GetSuggestionCount(), 20);
            SuggestionResult result2 = tree.GetSuggestions(new Word(new LatinAlphabet(), "aba"), 20, 999);
            Assert.AreEqual(result2.GetSuggestionCount(), 20);
            Assert.IsTrue(result.GetSuggestionArray().Select((word) => word.ToString()).SequenceEqual(result2.GetSuggestionArray().Select((word) => word.ToString())));
        }

        [TestMethod]
        public void GetSuggestionsOfWordNotContainedInTreeWithMaxDistanceParameterZero_ShouldReturnArrayContainingOnlyNullValues()
        {
            SuggestionResult result = tree.GetSuggestions(new Word(new LatinAlphabet(), "contai"), 20, 0);
            Assert.IsTrue(result.GetSuggestionCount() == 0);
            Assert.IsTrue(result.GetSuggestionArray().All(element => element == null));
        }

        [TestMethod]
        public void GetSuggestionForWordContainingCharactersNotPresentInWordTreeAlphabetWithMaxDistance_ShouldReturnFirst5AlphabeticallyOrderedWordsOfWordTree()
        {
            CustomAlphabet partOfArabicAlphabet = new CustomAlphabet("تزنيت".ToCharArray().Distinct().ToArray());
            SuggestionResult result = tree.GetSuggestions(new Word(partOfArabicAlphabet, "تزنيت"), 5, 999);
            Assert.IsTrue(result.GetSuggestionCount() == 5);
            Assert.IsTrue(result.GetSuggestionArray().Select((word) => word.ToString()).SequenceEqual(new string[] { "aah", "aalii", "aal", "aargh", "aa" } ));
        }

        [TestMethod]
        public void GetSuggestionsOfWordContainedInTreeWithMaxDistanceParameterZero_ShouldReturnArrayContainingOnlyExactMatchAndNullValues()
        {
            SuggestionResult result = tree.GetSuggestions(new Word(new LatinAlphabet(), "contain"), 20, 0);
            Assert.IsTrue(result.GetSuggestionCount() == 1);
            Assert.IsTrue(result.GetSuggestionArray()[0].ToString() == "contain");
            Assert.IsTrue(result.GetSuggestionArray().Take(new Range(1, 50)).All(element => element == null));
        }
    }
}
