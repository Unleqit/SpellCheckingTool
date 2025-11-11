using SpellCheckingTool;

namespace TestProject.Unit
{
    [TestClass]
    public class SuggestionTests
    {
#pragma warning disable CS8618 // tree is assigned in SetupTests() before the tests of this class get executed
        WordTree tree;
#pragma warning restore CS8618 

        [TestInitialize]
        public void SetupTests()
        {
            
        }

        [TestMethod]
        public void GetHundredSuggestionsOfAllDistances_ShouldReturnTenSuggestions()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word[] words = Word.ParseWords(alphabet, new string[] { "these", "are", "some", "random", "english", "words", "containing", "only", "latin", "characters" });
            tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(words);
            SuggestionResult result = tree.GetSuggestions("containing", 100, 999);
            Assert.AreEqual(result.GetSuggestionCount(), tree.metaData.wordCount);
            tree.Dispose();
        }
    }
}
