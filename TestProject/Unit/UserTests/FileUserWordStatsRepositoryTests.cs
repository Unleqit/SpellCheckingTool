using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.FilePersistence.Repositories;
using TestProject.Fakes;

namespace TestProject.Unit;

[TestClass]
public class FileUserWordStatsRepositoryTests
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

    private IUserWordStatsRepository CreateRepository()
    {
        return new FileUserWordStatsRepository(
            _paths,
            _alphabet,
            _userRepository,
            _reader,
            _writer);
    }

    [TestMethod]
    public void IncrementWord_ShouldCreateAndIncreaseUsageStats()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();
        repo.IncrementWord(user.Id, "hello");
        repo.IncrementWord(user.Id, "hello");

        var stats = repo.GetWordStats(user.Id).ToList();

        Assert.AreEqual(1, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(2, stats[0].UsageCount);
    }

    [TestMethod]
    public void IncrementWord_ShouldPersistStatsAcrossReload()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo1 = CreateRepository();
        repo1.IncrementWord(user.Id, "hello");
        repo1.IncrementWord(user.Id, "hello");
        repo1.IncrementWord(user.Id, "world");

        var repo2 = CreateRepository();
        var stats = repo2.GetWordStats(user.Id)
            .OrderBy(s => s.Word.ToString())
            .ToList();

        Assert.AreEqual(2, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(2, stats[0].UsageCount);
        Assert.AreEqual("world", stats[1].Word.ToString());
        Assert.AreEqual(1, stats[1].UsageCount);
    }

    [TestMethod]
    public void IncrementWord_ShouldNormalizeToLowercase()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();
        repo.IncrementWord(user.Id, "HeLLo");
        repo.IncrementWord(user.Id, "hello");

        var stats = repo.GetWordStats(user.Id).ToList();

        Assert.AreEqual(1, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(2, stats[0].UsageCount);
    }

    [TestMethod]
    public void DifferentUsers_ShouldHaveIsolatedStats()
    {
        var user1 = new User(Guid.NewGuid(), "armin", "HASH1", DateTime.UtcNow);
        var user2 = new User(Guid.NewGuid(), "zeynep", "HASH2", DateTime.UtcNow);

        _userRepository.Seed(user1);
        _userRepository.Seed(user2);

        var repo = CreateRepository();
        repo.IncrementWord(user1.Id, "hello");
        repo.IncrementWord(user1.Id, "hello");
        repo.IncrementWord(user2.Id, "world");

        var stats1 = repo.GetWordStats(user1.Id).ToList();
        var stats2 = repo.GetWordStats(user2.Id).ToList();

        Assert.AreEqual(1, stats1.Count);
        Assert.AreEqual("hello", stats1[0].Word.ToString());
        Assert.AreEqual(2, stats1[0].UsageCount);

        Assert.AreEqual(1, stats2.Count);
        Assert.AreEqual("world", stats2[0].Word.ToString());
        Assert.AreEqual(1, stats2[0].UsageCount);
    }

    [TestMethod]
    public void IncrementWord_ForUnknownUser_ShouldThrowError()
    {
        var repo = CreateRepository();

        Assert.ThrowsException<UserNotFoundDomainException>(() =>
            repo.IncrementWord(Guid.NewGuid(), "hello"));
    }

    [TestMethod]
    public void IncrementInvalidWord_ShouldThrowError()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();

        Assert.ThrowsException<InvalidWordCharacterException>(() =>
            repo.IncrementWord(user.Id, "hello!"));
    }

    [TestMethod]
    public void CreatingNewUser_ShouldInitializeEmptyStats()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var repo = CreateRepository();
        var stats = repo.GetWordStats(user.Id);

        Assert.IsNotNull(stats);
        Assert.AreEqual(0, stats.Count);
    }
}