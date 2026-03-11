using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Users;
using TestProject.Fakes;

namespace TestProject.Unit;

[TestClass]
public class UserServiceTests
{
    private FakeUserRepository _userRepo = null!;
    private SpyWordDataRepository _wordRepo = null!;
    private UserService _service = null!;
    private IAlphabet _alphabet = null!;

    [TestInitialize]
    public void Setup()
    {
        _alphabet = new LatinAlphabet();
        _userRepo = new FakeUserRepository();
        _wordRepo = new SpyWordDataRepository(_alphabet);
        _service = new UserService(_userRepo, _wordRepo, _wordRepo);
    }

    [TestMethod]
    public void Register_WithEmptyUsername_ShouldFail()
    {
        var result = _service.Register("", "HASH");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Username is required.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void Register_WithWhitespaceUsername_ShouldFail()
    {
        var result = _service.Register("   ", "HASH");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Username is required.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void Register_WithEmptyPassword_ShouldFail()
    {
        var result = _service.Register("armin", "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Password is required.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void Register_WithExistingUsername_ShouldFail()
    {
        _userRepo.Seed(new User(Guid.NewGuid(), "armin", "HASH1", DateTime.UtcNow));

        var result = _service.Register("armin", "HASH2");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Username already exists.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void Register_WithValidInput_ShouldSucceedAndStoreUser()
    {
        var result = _service.Register("armin", "HASH");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("armin", result.Value!.Username);
        Assert.AreEqual("HASH", result.Value.PasswordHash);

        var stored = _userRepo.GetByUsername("armin");
        Assert.IsNotNull(stored);
        Assert.AreEqual(result.Value.Id, stored!.Id);
    }

    [TestMethod]
    public void Register_ShouldTrimUsername()
    {
        var result = _service.Register("  armin  ", "HASH");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("armin", result.Value!.Username);
    }

    [TestMethod]
    public void Login_WithUnknownUser_ShouldFail()
    {
        var result = _service.Login("missing", "HASH");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void Login_WithWrongPassword_ShouldFail()
    {
        _userRepo.Seed(new User(Guid.NewGuid(), "armin", "RIGHT_HASH", DateTime.UtcNow));

        var result = _service.Login("armin", "WRONG_HASH");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Invalid password.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void Login_WithCorrectCredentials_ShouldSucceed()
    {
        var user = new User(Guid.NewGuid(), "armin", "RIGHT_HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.Login("armin", "RIGHT_HASH");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(user.Id, result.Value!.Id);
        Assert.AreEqual(user.Username, result.Value.Username);
    }

    [TestMethod]
    public void AddWord_WithUnknownUser_ShouldFail()
    {
        var result = _service.AddWord(Guid.NewGuid(), "salam");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
        Assert.AreEqual(0, _wordRepo.AddCustomWordCallCount);
        Assert.AreEqual(0, _wordRepo.IncrementWordCallCount);
    }

    [TestMethod]
    public void AddWord_WithEmptyWord_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.AddWord(user.Id, "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word is required.", result.ErrorMessage);
        Assert.AreEqual(0, _wordRepo.AddCustomWordCallCount);
    }

    [TestMethod]
    public void AddWord_ShouldWriteOnlyToCustomDictionary()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.AddWord(user.Id, "salam");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, _wordRepo.AddCustomWordCallCount);
        Assert.AreEqual(0, _wordRepo.IncrementWordCallCount);
        Assert.AreEqual(user.Id, _wordRepo.LastAddedCustomUserId);
        Assert.AreEqual("salam", _wordRepo.LastAddedCustomWord);
    }

    [TestMethod]
    public void RemoveWord_WithUnknownUser_ShouldFail()
    {
        var result = _service.RemoveWord(Guid.NewGuid(), "salam");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
        Assert.AreEqual(0, _wordRepo.RemoveCustomWordCallCount);
    }

    [TestMethod]
    public void RemoveWord_WithEmptyWord_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.RemoveWord(user.Id, "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word is required.", result.ErrorMessage);
        Assert.AreEqual(0, _wordRepo.RemoveCustomWordCallCount);
    }

    [TestMethod]
    public void RemoveWord_WhenWordIsMissing_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.RemoveWord(user.Id, "missing");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word not found in personal dictionary.", result.ErrorMessage);
        Assert.AreEqual(1, _wordRepo.RemoveCustomWordCallCount);
        Assert.AreEqual("missing", _wordRepo.LastRemovedCustomWord);
    }

    [TestMethod]
    public void RemoveWord_WhenWordExists_ShouldSucceed()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);
        _wordRepo.SeedCustomWord(user.Id, "salam");

        var result = _service.RemoveWord(user.Id, "salam");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, _wordRepo.RemoveCustomWordCallCount);
        Assert.AreEqual(user.Id, _wordRepo.LastRemovedCustomUserId);
        Assert.AreEqual("salam", _wordRepo.LastRemovedCustomWord);

        var remaining = _wordRepo.GetWords(user.Id).Select(w => w.ToString()).ToList();
        CollectionAssert.DoesNotContain(remaining, "salam");
    }

    [TestMethod]
    public void TrackWordUsage_WithUnknownUser_ShouldFail()
    {
        var result = _service.TrackWordUsage(Guid.NewGuid(), "hello");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
        Assert.AreEqual(0, _wordRepo.IncrementWordCallCount);
    }

    [TestMethod]
    public void TrackWordUsage_WithEmptyWord_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.TrackWordUsage(user.Id, "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word is required.", result.ErrorMessage);
        Assert.AreEqual(0, _wordRepo.IncrementWordCallCount);
    }

    [TestMethod]
    public void TrackWordUsage_ShouldWriteOnlyToStats()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.TrackWordUsage(user.Id, "hello");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, _wordRepo.IncrementWordCallCount);
        Assert.AreEqual(0, _wordRepo.AddCustomWordCallCount);
        Assert.AreEqual(user.Id, _wordRepo.LastIncrementUserId);
        Assert.AreEqual("hello", _wordRepo.LastIncrementWord);
    }

    [TestMethod]
    public void GetCustomWords_WithUnknownUser_ShouldFail()
    {
        var result = _service.GetCustomWords(Guid.NewGuid());

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void GetCustomWords_ShouldReturnOnlyCustomDictionaryWords()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        _wordRepo.SeedCustomWord(user.Id, "labas");
        _wordRepo.SeedCustomWord(user.Id, "salam");
        _wordRepo.SeedStat(user.Id, "hello", 3);

        var result = _service.GetCustomWords(user.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);

        var words = result.Value!.Select(w => w.ToString()).OrderBy(x => x).ToList();

        Assert.AreEqual(2, words.Count);
        CollectionAssert.AreEquivalent(new List<string> { "labas", "salam" }, words);
    }

    [TestMethod]
    public void GetStats_WithUnknownUser_ShouldFail()
    {
        var result = _service.GetStats(Guid.NewGuid());

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void GetStats_ShouldReturnTrackedStats()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        _wordRepo.SeedStat(user.Id, "hello", 3);
        _wordRepo.SeedStat(user.Id, "world", 2);
        _wordRepo.SeedCustomWord(user.Id, "salam");

        var result = _service.GetStats(user.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);

        var stats = result.Value!.OrderBy(s => s.Word.ToString()).ToList();

        Assert.AreEqual(2, stats.Count);

        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(3, stats[0].UsageCount);

        Assert.AreEqual("world", stats[1].Word.ToString());
        Assert.AreEqual(2, stats[1].UsageCount);
    }

    [TestMethod]
    public void AddWord_ShouldNormalizeAndStoreCaseInsensitiveWord()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        var result = _service.AddWord(user.Id, "SaLaM");

        Assert.IsTrue(result.Success);

        var words = _wordRepo.GetWords(user.Id).Select(w => w.ToString()).ToList();
        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void TrackWordUsage_MultipleTimes_ShouldIncreaseUsageCount()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepo.Seed(user);

        _service.TrackWordUsage(user.Id, "hello");
        _service.TrackWordUsage(user.Id, "hello");
        _service.TrackWordUsage(user.Id, "hello");

        var stats = _wordRepo.GetWordStats(user.Id).ToList();

        Assert.AreEqual(1, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
        Assert.AreEqual(3, stats[0].UsageCount);
    }
}