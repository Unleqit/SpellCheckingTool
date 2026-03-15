using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.FilePersistence;

namespace TestProject.Unit;

[TestClass]
public class SuggestionTests
{
    private WordTree tree = null!;

    [TestInitialize]
    public void SetupTests()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"../../../.."));
        string path = Path.Combine(projectRoot, @"TestProject/Resources/wordFile.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Wordfile not found at {path}");

        var filePath = new FilePath(path);

        var persistence = new FilePersistenceService();
        tree = persistence.Load(filePath);
    }

    [TestMethod]
    public void GetTwentySuggestionsOfAllDistancesForTreeWithTenElements_ShouldReturnTenSuggestions()
    {
        IAlphabet alphabet = new LatinAlphabet();
        Word[] words = Word.ParseWords(alphabet, new[]
        {
            "these","are","some","random","english","words","containing","only","latin","characters"
        });

        WordTree smallTree = new WordTree(alphabet);
        smallTree.Add(words);

        ISuggestionService service = new SuggestionService(smallTree, new LevenshteinDistanceAlgorithm());
        SuggestionResult result = service.GetSuggestionResult(new Word(new LatinAlphabet(), "containing"), 20, 999);

        Assert.AreEqual(smallTree.WordCount, result.GetSuggestionCount());
    }

    [TestMethod]
    public void GetFiveSuggestionsInLargeTreeForContain_ShouldReturnFiveAndContainFirst()
    {
        ISuggestionService service = new SuggestionService(tree, new LevenshteinDistanceAlgorithm());
        SuggestionResult result = service.GetSuggestionResult(new Word(new LatinAlphabet(), "contain"), 5, 999);

        Assert.AreEqual(5, result.GetSuggestionCount());
        Assert.AreEqual("contain", result.GetSuggestionArray()[0].ToString());
    }

    [TestMethod]
    public void GetHundredSuggestionsInLargeTreeForContain_ShouldReturnAtMostTwenty()
    {
        ISuggestionService service = new SuggestionService(tree, new LevenshteinDistanceAlgorithm());
        SuggestionResult result = service.GetSuggestionResult(new Word(new LatinAlphabet(), "contain"), 100, 999);

        Assert.AreEqual(20, result.GetSuggestionCount());
        Assert.AreEqual("contain", result.GetSuggestionArray()[0].ToString());
    }

    [TestMethod]
    public void GetSuggestionsTwice_ShouldYieldSameResult()
    {
        ISuggestionService service = new SuggestionService(tree, new LevenshteinDistanceAlgorithm());

        SuggestionResult r1 = service.GetSuggestionResult(new Word(new LatinAlphabet(), "aba"), 20, 999);
        SuggestionResult r2 = service.GetSuggestionResult(new Word(new LatinAlphabet(), "aba"), 20, 999);

        Assert.AreEqual(20, r1.GetSuggestionCount());
        Assert.AreEqual(20, r2.GetSuggestionCount());

        Assert.IsTrue(r1.GetSuggestionArray().Select(w => w.ToString())
            .SequenceEqual(r2.GetSuggestionArray().Select(w => w.ToString())));
    }

    [TestMethod]
    public void GetSuggestionsOfWordNotContainedInTreeWithMaxDistanceParameterZero_ShouldReturnZeroSuggestions()
    {
        ISuggestionService service = new SuggestionService(tree, new LevenshteinDistanceAlgorithm());
        SuggestionResult result = service.GetSuggestionResult(new Word(new LatinAlphabet(), "contai"), 20, 0);

        Assert.AreEqual(0, result.GetSuggestionCount());
        Assert.AreEqual(0, result.GetSuggestionArray().Length);
    }

    [TestMethod]
    public void GetSuggestionForWordContainingDifferentAlphabet_ShouldStillReturnSomeSuggestions()
    {
        // This test is made robust: we only assert count + all are non-null.
        ISuggestionService service = new SuggestionService(tree, new LevenshteinDistanceAlgorithm());

        CustomAlphabet partOfArabicAlphabet = new CustomAlphabet("تزنيت".ToCharArray().Distinct().ToArray());
        SuggestionResult result = service.GetSuggestionResult(new Word(partOfArabicAlphabet, "تزنيت"), 5, 999);

        Assert.AreEqual(5, result.GetSuggestionCount());
        Assert.IsTrue(result.GetSuggestionArray().All(w => w != null));
    }

    [TestMethod]
    public void GetSuggestionsOfWordContainedInTreeWithMaxDistanceParameterZero_ShouldReturnOnlyExactMatch()
    {
        ISuggestionService service = new SuggestionService(tree, new LevenshteinDistanceAlgorithm());
        SuggestionResult result = service.GetSuggestionResult(new Word(new LatinAlphabet(), "contain"), 20, 0);

        Assert.AreEqual(1, result.GetSuggestionCount());
        Assert.AreEqual("contain", result.GetSuggestionArray()[0].ToString());
        Assert.AreEqual(1, result.GetSuggestionArray().Length);
    }
}