using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Infrastructure.FilePersistence.Repositories;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.UserPersistence;
using TestProject.Fakes;

namespace TestProject.Unit.UserTests;

[TestClass]
public class FileUserCustomDictionaryRepositoryTests
{
    private string _testDirectory = null!;
    private IAlphabet _alphabet = null!;
    private UserStorePaths _paths = null!;
    private UserStoreJsonReader _reader = null!;
    private UserStoreJsonWriter _writer = null!;
    private FakeUserRepository _userRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _alphabet = new LatinAlphabet();
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "SpellCheckingToolTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testDirectory);

        _paths = new UserStorePaths(_testDirectory);
        _reader = new UserStoreJsonReader();
        _writer = new UserStoreJsonWriter();
        _userRepository = new FakeUserRepository();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    private IUserCustomDictionaryRepository CreateRepository()
    {
        return new FileUserCustomDictionaryRepository(
            _paths,
            _alphabet,
            _userRepository,
            _reader,
            _writer);
    }

    [TestMethod]
    public void AddCustomWord_ShouldPersistWordAndLoadAgain()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo1 = CreateRepository();
        repo1.AddWord(user.Id, "salam");

        var repo2 = CreateRepository();
        var words = repo2.GetWords(user.Id).Select(w => w.ToString()).ToList();

        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void AddCustomWord_ShouldNormalizeToLowercase()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();
        repo.AddWord(user.Id, "SaLaM");

        var words = repo.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.AreEqual(1, words.Count);
        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void AddSameCustomWordTwice_ShouldNotDuplicateWord()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();
        repo.AddWord(user.Id, "salam");
        repo.AddWord(user.Id, "SALAM");

        var words = repo.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.AreEqual(1, words.Count);
        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void RemoveCustomWord_ShouldDeleteWord()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();
        repo.AddWord(user.Id, "salam");

        bool removed = repo.RemoveWord(user.Id, "salam");
        var words = repo.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.IsTrue(removed);
        Assert.AreEqual(0, words.Count);
    }

    [TestMethod]
    public void RemoveCustomWord_ShouldPersistDeletionAcrossReload()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo1 = CreateRepository();
        repo1.AddWord(user.Id, "salam");
        repo1.RemoveWord(user.Id, "salam");

        var repo2 = CreateRepository();
        var words = repo2.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.AreEqual(0, words.Count);
    }

    [TestMethod]
    public void RemoveMissingCustomWord_ShouldReturnFalse()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();

        bool removed = repo.RemoveWord(user.Id, "missing");

        Assert.IsFalse(removed);
    }

    [TestMethod]
    public void DifferentUsers_ShouldHaveIsolatedCustomDictionaries()
    {
        var user1 = new User(Guid.NewGuid(), "armin", "HASH1", DateTime.UtcNow);
        var user2 = new User(Guid.NewGuid(), "zeynep", "HASH2", DateTime.UtcNow);

        _userRepository.Seed(user1);
        _userRepository.Seed(user2);

        var repo = CreateRepository();
        repo.AddWord(user1.Id, "salam");
        repo.AddWord(user2.Id, "labas");

        var words1 = repo.GetWords(user1.Id).Select(w => w.ToString()).ToList();
        var words2 = repo.GetWords(user2.Id).Select(w => w.ToString()).ToList();

        CollectionAssert.Contains(words1, "salam");
        CollectionAssert.DoesNotContain(words1, "labas");

        CollectionAssert.Contains(words2, "labas");
        CollectionAssert.DoesNotContain(words2, "salam");
    }

    [TestMethod]
    public void AddCustomWord_ForUnknownUser_ShouldThrowError()
    {
        var repo = CreateRepository();

        Assert.ThrowsException<UserNotFoundDomainException>(() =>
            repo.AddWord(Guid.NewGuid(), "salam"));
    }

    [TestMethod]
    public void RemoveCustomWord_ForUnknownUser_ShouldThrowError()
    {
        var repo = CreateRepository();

        Assert.ThrowsException<UserNotFoundDomainException>(() =>
            repo.RemoveWord(Guid.NewGuid(), "salam"));
    }

    [TestMethod]
    public void AddInvalidCustomWord_ShouldThrowError()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();

        Assert.ThrowsException<InvalidWordCharacterException>(() =>
            repo.AddWord(user.Id, "hello!"));
    }

    [TestMethod]
    public void CreatingNewUser_ShouldInitializeEmptyDictionary()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();
        var words = repo.GetWords(user.Id);

        Assert.IsNotNull(words);
        Assert.AreEqual(0, words.Count);
    }
}