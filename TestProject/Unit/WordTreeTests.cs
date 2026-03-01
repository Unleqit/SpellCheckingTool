using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using System.Diagnostics;

namespace TestProject.Unit;

[TestClass]
public class WordTreeTests
{
    [TestMethod]
    public void CreateNewWordTree_ShouldInitialize()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);
        Assert.IsNotNull(tree);
    }

    [TestMethod]
    public void ParseValidWord_ShouldCreateWord()
    {
        string rawWord = "test";
        IAlphabet alphabet = new LatinAlphabet();
        Word parsedWord = new Word(alphabet, rawWord);

        Assert.IsNotNull(parsedWord);
        Assert.AreEqual(rawWord, parsedWord.ToString());
    }

    [TestMethod]
    public void ParseInvalidWord_ShouldFail()
    {
        string rawWord = "test\n";
        IAlphabet alphabet = new LatinAlphabet();

        Assert.ThrowsException<Exception>(() => new Word(alphabet, rawWord));
    }

    [TestMethod]
    public void ParseMultipleWords_ShouldSucceed()
    {
        string[] rawWords = { "test", "anotherWord" };
        IAlphabet alphabet = new LatinAlphabet();

        Word[] words = Word.ParseWords(alphabet, rawWords);
        Assert.AreEqual(rawWords.Length, words.Length);
    }

    [TestMethod]
    public void ParseMultipleWordsIncludingInvalidWords_ShouldOnlyReturnArrayOfValidWords()
    {
        string[] rawWords = { "test", "anotherWord", "ungültigesWort" };
        IAlphabet alphabet = new LatinAlphabet();

        Word[] words = Word.ParseWords(alphabet, rawWords);
        Assert.AreEqual(rawWords.Length - 1, words.Length);
    }

    //TODO: Add tests for adding/searching/removing from the tree

    [TestMethod]
    public void AddWordToTree_ShouldReturnSuccessCount()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        int successCount = tree.Add(new Word(alphabet, "test"));

        Assert.AreEqual(1, successCount);
        Assert.AreEqual(1, tree.WordCount);
    }

    [TestMethod]
    public void AddWordAlreadyContainedInTree_ShouldNotAddWord()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        Assert.AreEqual(1, tree.Add(new Word(alphabet, "test")));
        Assert.AreEqual(1, tree.WordCount);

        Assert.AreEqual(0, tree.Add(new Word(alphabet, "test")));
        Assert.AreEqual(1, tree.WordCount);
    }

    [TestMethod]
    public void SearchWordContainedInTree_ShouldReturnTrue()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        Word w = new Word(alphabet, "test");
        Assert.AreEqual(1, tree.Add(w));

        bool result = tree.Contains(w);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void SearchWordNotContainedInTree_ShouldReturnFalse()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        tree.Add(new Word(alphabet, "test"));
        bool result = tree.Contains(new Word(alphabet, "someOtherWord"));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RemoveWordContainedInTree_ShouldRemoveWordAndReturnSuccess()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        Word w = new Word(alphabet, "test");
        tree.Add(w);

        int removeSuccessCount = tree.Remove(w);

        Assert.AreEqual(1, removeSuccessCount);
        Assert.AreEqual(0, tree.WordCount);
    }

    [TestMethod]
    public void RemoveWordNotContainedInTree_ShouldReturnZero()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        tree.Add(new Word(alphabet, "test"));
        int removeSuccessCount = tree.Remove(new Word(alphabet, "someOtherWord"));

        Assert.AreEqual(0, removeSuccessCount);
        Assert.AreEqual(1, tree.WordCount);
    }

    [TestMethod]
    public void RemoveShorterWord_ShouldKeepLongerWordAndOnlyRemoveShorterOne()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        Word w = new Word(alphabet, "lol");
        Word w2 = new Word(alphabet, "lollipop");

        Assert.AreEqual(2, tree.Add(new[] { w, w2 }));
        Assert.AreEqual(2, tree.WordCount);

        Assert.AreEqual(1, tree.Remove(w));
        Assert.AreEqual(1, tree.WordCount);

        Assert.IsFalse(tree.Contains(w));
        Assert.IsTrue(tree.Contains(w2));
    }

    [TestMethod]
    public void RemoveLongerWord_ShouldKeepShorterWordAndOnlyRemoveLongerOne()
    {
        IAlphabet alphabet = new LatinAlphabet();
        WordTree tree = new WordTree(alphabet);

        Word w = new Word(alphabet, "lol");
        Word w2 = new Word(alphabet, "lollipop");

        Assert.AreEqual(2, tree.Add(new[] { w, w2 }));
        Assert.AreEqual(2, tree.WordCount);

        Assert.AreEqual(1, tree.Remove(w2));
        Assert.AreEqual(1, tree.WordCount);

        Assert.IsFalse(tree.Contains(w2));
        Assert.IsTrue(tree.Contains(w));
    }

    [TestMethod]
    public void ParseProductionSizedWordTreeFromStringArray_ShouldTakeLessThanTwoSecondsToComplete()
    {
        string resourceDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"../../../Resources/"));
        string[] rawWords = File.ReadAllText(Path.Combine(resourceDirectory, "wordfile.txt"))
            .Replace("\r\n", "\n")
            .Split("\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.IsTrue(rawWords.Length > 0);

        Stopwatch sw = Stopwatch.StartNew();

        IAlphabet alphabet = new LatinAlphabet();
        Word[] parsedWords = Word.ParseWords(alphabet, rawWords);

        WordTree tree = new WordTree(alphabet);
        tree.Add(parsedWords);

        Assert.AreEqual(rawWords.Length, tree.WordCount);

        sw.Stop();
        Assert.IsTrue(sw.ElapsedMilliseconds < 2000);
    }
}