using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence;

namespace TestProject.Unit;

[TestClass]
public class FileUserStoreTests
{
    private string _testDirectory = null!;
    private IAlphabet _alphabet = null!;

    [TestInitialize]
    public void Setup()
    {
        _alphabet = new LatinAlphabet();
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "SpellCheckingToolTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [TestMethod]
    public void AddUser_ShouldPersistUserAndLoadAgain()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store1 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store1.Add(user);

        var store2 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        var loaded = store2.GetById(user.Id);

        Assert.IsNotNull(loaded);
        Assert.AreEqual(user.Id, loaded!.Id);
        Assert.AreEqual("armin", loaded.Username);
        Assert.AreEqual("HASH", loaded.PasswordHash);
    }

    [TestMethod]
    public void GetByUsername_ShouldFindUserCaseInsensitive()
    {
        var user = new User(Guid.NewGuid(), "Armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);

        var loaded = store.GetByUsername("armin");

        Assert.IsNotNull(loaded);
        Assert.AreEqual(user.Id, loaded!.Id);
    }

    [TestMethod]
    public void AddDuplicateUserId_ShouldThrowError()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);

        Assert.ThrowsException<InvalidOperationException>(() => store.Add(user));
    }

    [TestMethod]
    public void AddDuplicateUsername_ShouldThrowError()
    {
        var user1 = new User(Guid.NewGuid(), "armin", "HASH1", DateTime.UtcNow);
        var user2 = new User(Guid.NewGuid(), "Armin", "HASH2", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user1);

        Assert.ThrowsException<InvalidOperationException>(() => store.Add(user2));
    }

    [TestMethod]
    public void AddCustomWord_ShouldPersistWordAndLoadAgain()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store1 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store1.Add(user);
        store1.AddWord(user.Id, "salam");

        var store2 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        var words = store2.GetWords(user.Id).Select(w => w.ToString()).ToList();

        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void AddCustomWord_ShouldNormalizeToLowercase()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);
        store.AddWord(user.Id, "SaLaM");

        var words = store.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.AreEqual(1, words.Count);
        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void AddSameCustomWordTwice_ShouldNotDuplicateWord()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);
        store.AddWord(user.Id, "salam");
        store.AddWord(user.Id, "SALAM");

        var words = store.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.AreEqual(1, words.Count);
        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void RemoveCustomWord_ShouldDeleteWord()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);
        store.AddWord(user.Id, "salam");

        bool removed = store.RemoveWord(user.Id, "salam");
        var words = store.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.IsTrue(removed);
        Assert.AreEqual(0, words.Count);
    }

    [TestMethod]
    public void RemoveCustomWord_ShouldPersistDeletionAcrossReload()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store1 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store1.Add(user);
        store1.AddWord(user.Id, "salam");
        store1.RemoveWord(user.Id, "salam");

        var store2 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        var words = store2.GetWords(user.Id).Select(w => w.ToString()).ToList();

        Assert.AreEqual(0, words.Count);
    }

    [TestMethod]
    public void RemoveMissingCustomWord_ShouldReturnFalse()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);

        bool removed = store.RemoveWord(user.Id, "missing");

        Assert.IsFalse(removed);
    }

    [TestMethod]
    public void IncrementWord_ShouldCreateAndIncreaseUsageStats()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);
        store.IncrementWord(user.Id, "hello");
        store.IncrementWord(user.Id, "hello");

        var stats = store.GetWordStats(user.Id).ToList();

        Assert.AreEqual(1, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(2, stats[0].UsageCount);
    }

    [TestMethod]
    public void IncrementWord_ShouldPersistStatsAcrossReload()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store1 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store1.Add(user);
        store1.IncrementWord(user.Id, "hello");
        store1.IncrementWord(user.Id, "hello");
        store1.IncrementWord(user.Id, "world");

        var store2 = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        var stats = store2.GetWordStats(user.Id)
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

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);
        store.IncrementWord(user.Id, "HeLLo");
        store.IncrementWord(user.Id, "hello");

        var stats = store.GetWordStats(user.Id).ToList();

        Assert.AreEqual(1, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(2, stats[0].UsageCount);
    }

    [TestMethod]
    public void CustomDictionaryAndStats_ShouldBeSeparated()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);

        store.AddWord(user.Id, "salam");
        store.IncrementWord(user.Id, "hello");
        store.IncrementWord(user.Id, "hello");

        var customWords = store.GetWords(user.Id).Select(w => w.ToString()).ToList();
        var stats = store.GetWordStats(user.Id).ToList();

        Assert.AreEqual(1, customWords.Count);
        CollectionAssert.Contains(customWords, "salam");

        Assert.AreEqual(1, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(2, stats[0].UsageCount);
    }

    [TestMethod]
    public void DifferentUsers_ShouldHaveIsolatedCustomDictionaries()
    {
        var user1 = new User(Guid.NewGuid(), "armin", "HASH1", DateTime.UtcNow);
        var user2 = new User(Guid.NewGuid(), "zeynep", "HASH2", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user1);
        store.Add(user2);

        store.AddWord(user1.Id, "salam");
        store.AddWord(user2.Id, "labas");

        var words1 = store.GetWords(user1.Id).Select(w => w.ToString()).ToList();
        var words2 = store.GetWords(user2.Id).Select(w => w.ToString()).ToList();

        CollectionAssert.Contains(words1, "salam");
        CollectionAssert.DoesNotContain(words1, "labas");

        CollectionAssert.Contains(words2, "labas");
        CollectionAssert.DoesNotContain(words2, "salam");
    }

    [TestMethod]
    public void DifferentUsers_ShouldHaveIsolatedStats()
    {
        var user1 = new User(Guid.NewGuid(), "armin", "HASH1", DateTime.UtcNow);
        var user2 = new User(Guid.NewGuid(), "zeynep", "HASH2", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user1);
        store.Add(user2);

        store.IncrementWord(user1.Id, "hello");
        store.IncrementWord(user1.Id, "hello");
        store.IncrementWord(user2.Id, "world");

        var stats1 = store.GetWordStats(user1.Id).ToList();
        var stats2 = store.GetWordStats(user2.Id).ToList();

        Assert.AreEqual(1, stats1.Count);
        Assert.AreEqual("hello", stats1[0].Word.ToString());
        Assert.AreEqual(2, stats1[0].UsageCount);

        Assert.AreEqual(1, stats2.Count);
        Assert.AreEqual("world", stats2[0].Word.ToString());
        Assert.AreEqual(1, stats2[0].UsageCount);
    }

    [TestMethod]
    public void AddCustomWord_ForUnknownUser_ShouldThrowError()
    {
        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);

        Assert.ThrowsException<KeyNotFoundException>(() =>
            store.AddWord(Guid.NewGuid(), "salam"));
    }

    [TestMethod]
    public void RemoveCustomWord_ForUnknownUser_ShouldThrowError()
    {
        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);

        Assert.ThrowsException<KeyNotFoundException>(() =>
            store.RemoveWord(Guid.NewGuid(), "salam"));
    }

    [TestMethod]
    public void IncrementWord_ForUnknownUser_ShouldThrowError()
    {
        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);

        Assert.ThrowsException<KeyNotFoundException>(() =>
            store.IncrementWord(Guid.NewGuid(), "hello"));
    }

    [TestMethod]
    public void AddInvalidCustomWord_ShouldThrowError()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);

        Assert.ThrowsException<Exception>(() => store.AddWord(user.Id, "hello!"));
    }

    [TestMethod]
    public void IncrementInvalidWord_ShouldThrowError()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);

        Assert.ThrowsException<Exception>(() => store.IncrementWord(user.Id, "hello!"));
    }

    [TestMethod]
    public void CreatingNewUser_ShouldInitializeEmptyStatsAndDictionary()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var userSettingsRepository = new FileUserSettingsRepository(_testDirectory);
        var store = new FileUserStore(_testDirectory, _alphabet, userSettingsRepository);
        store.Add(user);

        var words = store.GetWords(user.Id);
        var stats = store.GetWordStats(user.Id);

        Assert.IsNotNull(words);
        Assert.IsNotNull(stats);
        Assert.AreEqual(0, words.Count);
        Assert.AreEqual(0, stats.Count);
    }
}