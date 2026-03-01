using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.FilePersistence;

namespace TestProject.Unit;

[TestClass]
public class PersistenceServiceTests
{
    private WordTree tree = null!;

    [TestInitialize]
    public void SetupTests()
    {
        IAlphabet alphabet = new LatinAlphabet();
        Word[] words = Word.ParseWords(alphabet, new[]
        {
            "these","are","some","random","english","words","containing","only","latin","characters"
        });

        tree = new WordTree(alphabet);
        tree.Add(words);
    }

    [TestMethod]
    public void SerializeAndDeserializeWordTree_ShouldSucceed()
    {
        string file = Path.Combine(Directory.GetCurrentDirectory(), "test.json");
        var filePath = new FilePath(file);

        var persistence = new FilePersistenceService();

        bool success = persistence.Save(tree, filePath);
        Assert.IsTrue(success);
        Assert.IsTrue(File.Exists(file));

        WordTree tree2 = persistence.Load(filePath);

        Assert.IsNotNull(tree2);
        Assert.AreEqual(tree.WordBufferLength, tree2.WordBufferLength);
        Assert.AreEqual(tree.WordCount, tree2.WordCount);
        Assert.IsTrue(tree.Alphabet.GetChars().SequenceEqual(tree2.Alphabet.GetChars()));
        Assert.AreEqual(tree.Alphabet.GetLength(), tree2.Alphabet.GetLength());
        Assert.AreEqual(tree.Contains(new Word(tree.Alphabet, "these")), tree2.Contains(new Word(tree2.Alphabet, "these")));

        File.Delete(file);
        Assert.IsFalse(File.Exists(file));
    }

    [TestMethod]
    public void CreateWordFilePathWithInvalidPath_ShouldThrowException()
    {
        Assert.ThrowsException<Exception>(() => new FilePath(@"this/is/an/invalid/filepath"));
    }

    [TestMethod]
    public void CreateWordFilePathInDirectory_ShouldThrowException()
    {
        Assert.ThrowsException<Exception>(() => new FilePath(Directory.GetCurrentDirectory() + @"\"));
    }

    [TestMethod]
    public void SaveToNonJsonFile_ShouldThrowException()
    {
        var persistence = new FilePersistenceService();
        Assert.ThrowsException<Exception>(() =>
            persistence.Save(tree, new FilePath(Path.Combine(Directory.GetCurrentDirectory(), "thisIsNotAValidFileFormat.txt"))));
    }

    [TestMethod]
    public void DeserializeNonExistantFile_ShouldThrowException()
    {
        var persistence = new FilePersistenceService();

        string file = Path.Combine(Directory.GetCurrentDirectory(), "does_not_exist.json");
        var filePath = new FilePath(file);

        Assert.ThrowsException<FileNotFoundException>(() => persistence.Load(filePath));
    }

    [TestMethod]
    public void DeserializeTreeFromNonJsonFile_ShouldThrowException()
    {
        // create invalid file
        string file = Path.Combine(Directory.GetCurrentDirectory(), "thisIsNotAValidFileFormat.txt");
        File.WriteAllText(file, "");
        Assert.IsTrue(File.Exists(file));

        var persistence = new FilePersistenceService();
        Assert.ThrowsException<Exception>(() => persistence.Load(new FilePath(file)));

        File.Delete(file);
        Assert.IsFalse(File.Exists(file));
    }
}