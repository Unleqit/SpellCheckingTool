using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.Suggestions;

namespace TestProject.Unit;

[TestClass]
public class DistanceAlgorithmTests
{
    [TestMethod]
    public void LevenshteinDistanceMatchingResizeWordTree_ShouldNotThrow()
    {
        string word1 = "example";
        string word2 = "longerExample";

        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        tree.Add(new Word(alphabet, word1));
        var algo = new LevenshteinDistanceAlgorithm(tree);

        tree.Add(new Word(alphabet, word2));

        // should not throw; no assert needed (test passes if no exception)
        _ = algo.GetDistance(new Word(alphabet, word1), new Word(alphabet, word2));
    }

    [TestMethod]
    public void LevenshteinDistanceMatchingMatchSameWord_ShouldReturnZero()
    {
        IAlphabet alphabet = new LatinAlphabet();
        Word wordA = new Word(alphabet, "test");
        Word wordB = new Word(alphabet, "test");

        WordTree tree = new WordTree(alphabet);
        tree.Add(wordA);

        var algo = new LevenshteinDistanceAlgorithm(tree);

        int distance = algo.GetDistance(wordA, wordB);
        Assert.AreEqual(0, distance);
    }

    [TestMethod]
    public void LevenshteinDistanceMatchingMatchTestAndText_ShouldReturnOne()
    {
        IAlphabet alphabet = new LatinAlphabet();
        Word wordA = new Word(alphabet, "test");
        Word wordB = new Word(alphabet, "text");

        WordTree tree = new WordTree(alphabet);
        tree.Add(wordA);

        var algo = new LevenshteinDistanceAlgorithm(tree);

        int distance = algo.GetDistance(wordA, wordB);
        Assert.AreEqual(1, distance);
    }

    [TestMethod]
    public void LevenshteinDistanceMatchingMatchInterestingAndImplementation_ShouldReturnNine()
    {
        IAlphabet alphabet = new LatinAlphabet();
        Word wordA = new Word(alphabet, "interesting");
        Word wordB = new Word(alphabet, "implementation");

        WordTree tree = new WordTree(alphabet);
        tree.Add(wordA);

        var algo = new LevenshteinDistanceAlgorithm(tree);

        int distance = algo.GetDistance(wordA, wordB);
        Assert.AreEqual(9, distance);
    }
}