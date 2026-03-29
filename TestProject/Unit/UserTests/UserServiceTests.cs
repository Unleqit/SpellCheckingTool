using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Users;
using TestProject.Fakes;

namespace TestProject.Unit;

[TestClass]
public class UserServiceTests
{
    private FakeUserRepository _userRepository = null!;
    private FakeUserWordStatsRepository _wordStatsRepository = null!;
    private FakeUserCustomDictionaryRepository _customDictionaryRepository = null!;
    private FakeUserSettingsRepository _settingsRepository = null!;
    private UserService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        var alphabet = new LatinAlphabet();

        _userRepository = new FakeUserRepository();
        _wordStatsRepository = new FakeUserWordStatsRepository(alphabet);
        _customDictionaryRepository = new FakeUserCustomDictionaryRepository(alphabet);
        _settingsRepository = new FakeUserSettingsRepository();

        _service = new UserService(
            _userRepository,
            _wordStatsRepository,
            _customDictionaryRepository,
            _settingsRepository
        );
    }

    [TestMethod]
    public void AddWord_ShouldDelegateToCustomDictionaryRepository()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var result = _service.AddWord(user.Id, "Salam");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, _customDictionaryRepository.AddCustomWordCallCount);
        Assert.AreEqual(user.Id, _customDictionaryRepository.LastAddedCustomUserId);
        Assert.AreEqual("Salam", _customDictionaryRepository.LastAddedCustomWord);

        var words = _customDictionaryRepository.GetWords(user.Id)
            .Select(w => w.ToString())
            .ToList();

        CollectionAssert.Contains(words, "salam");
    }

    [TestMethod]
    public void AddWord_ForUnknownUser_ShouldFail()
    {
        var result = _service.AddWord(Guid.NewGuid(), "salam");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
    }

    [TestMethod]
    public void AddWord_WithEmptyWord_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var result = _service.AddWord(user.Id, "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word is required.", result.ErrorMessage);
    }

    [TestMethod]
    public void RemoveWord_ShouldDelegateToCustomDictionaryRepository()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);
        _customDictionaryRepository.SeedCustomWord(user.Id, "salam");

        var result = _service.RemoveWord(user.Id, "salam");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, _customDictionaryRepository.RemoveCustomWordCallCount);
        Assert.AreEqual(user.Id, _customDictionaryRepository.LastRemovedCustomUserId);
        Assert.AreEqual("salam", _customDictionaryRepository.LastRemovedCustomWord);
    }

    [TestMethod]
    public void RemoveWord_ForUnknownUser_ShouldFail()
    {
        var result = _service.RemoveWord(Guid.NewGuid(), "salam");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
    }

    [TestMethod]
    public void RemoveWord_WithEmptyWord_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var result = _service.RemoveWord(user.Id, "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word is required.", result.ErrorMessage);
    }

    [TestMethod]
    public void RemoveWord_WhenWordDoesNotExist_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var result = _service.RemoveWord(user.Id, "missing");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word not found in personal dictionary.", result.ErrorMessage);
    }

    [TestMethod]
    public void TrackWordUsage_ShouldDelegateToWordStatsRepository()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var result = _service.TrackWordUsage(user.Id, "Hello");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, _wordStatsRepository.IncrementWordCallCount);
        Assert.AreEqual(user.Id, _wordStatsRepository.LastIncrementUserId);
        Assert.AreEqual("Hello", _wordStatsRepository.LastIncrementWord);

        var stats = _wordStatsRepository.GetWordStats(user.Id).ToList();
        Assert.AreEqual(1, stats.Count);
        Assert.AreEqual("hello", stats[0].Word.ToString());
    }

    [TestMethod]
    public void TrackWordUsage_ForUnknownUser_ShouldFail()
    {
        var result = _service.TrackWordUsage(Guid.NewGuid(), "hello");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
    }

    [TestMethod]
    public void TrackWordUsage_WithEmptyWord_ShouldFail()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);

        var result = _service.TrackWordUsage(user.Id, "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Word is required.", result.ErrorMessage);
    }

    [TestMethod]
    public void GetCustomWords_ShouldReturnWordsFromCustomDictionaryRepository()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);
        _customDictionaryRepository.SeedCustomWord(user.Id, "salam");
        _customDictionaryRepository.SeedCustomWord(user.Id, "labas");

        var result = _service.GetCustomWords(user.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);

        var words = result.Value!.Select(w => w.ToString()).ToList();
        CollectionAssert.Contains(words, "salam");
        CollectionAssert.Contains(words, "labas");
    }

    [TestMethod]
    public void GetCustomWords_ForUnknownUser_ShouldFail()
    {
        var result = _service.GetCustomWords(Guid.NewGuid());

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
    }

    [TestMethod]
    public void GetStats_ShouldReturnStatsFromWordStatsRepository()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);
        _userRepository.Seed(user);
        _wordStatsRepository.SeedStat(user.Id, "hello", 2);
        _wordStatsRepository.SeedStat(user.Id, "world", 1);

        var result = _service.GetStats(user.Id);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);

        var stats = result.Value!.ToList();
        Assert.AreEqual(2, stats.Count);
        Assert.IsTrue(stats.Any(s => s.Word.ToString() == "hello" && s.UsageCount == 2));
        Assert.IsTrue(stats.Any(s => s.Word.ToString() == "world" && s.UsageCount == 1));
    }

    [TestMethod]
    public void GetStats_ForUnknownUser_ShouldFail()
    {
        var result = _service.GetStats(Guid.NewGuid());

        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.ErrorMessage);
    }
}