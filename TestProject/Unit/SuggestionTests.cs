using SpellCheckingTool;

namespace TestProject.Unit
{
    [TestClass]
    public class SuggestionTests
    {
        WordTree tree;

        [TestInitialize]
        public void SetupTests()
        {
           
        }

        [TestMethod]
        public void GetHundredSuggestionsOfAllDistances_ShouldReturnTenSuggestions()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word[] words = Word.ParseWords(alphabet, new string[] { "these", "are", "some", "random", "english", "words", "containing", "only", "latin", "characters" });
            tree = new WordTree(alphabet, new FilePersistenceService());
            tree.Add(words);

            SuggestionResult result = tree.GetSuggestions("containing", 100, 999);
            Assert.AreEqual(result.GetSuggestionCount(), tree.metaData.wordCount);
        }
    }
}
