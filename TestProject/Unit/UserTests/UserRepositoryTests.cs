using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Infrastructure.FilePersistence.Repositories;
using SpellCheckingTool.Infrastructure.UserPersistence;

namespace TestProject.Unit.UserTests;

[TestClass]
public class UserRepositoryTests
{
    private string _testDirectory = null!;
    private UserStorePaths _paths = null!;
    private UserStoreJsonSerializer _serializer = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "SpellCheckingToolTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testDirectory);

        _paths = new UserStorePaths(_testDirectory);
        _serializer = new UserStoreJsonSerializer();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    private IUserRepository CreateRepository()
    {
        return new UserRepository(_paths, _serializer);
    }

    [TestMethod]
    public void AddUser_ShouldPersistUserAndLoadAgain()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var repo1 = CreateRepository();
        repo1.Add(user);

        var repo2 = CreateRepository();
        var loaded = repo2.GetById(user.Id);

        Assert.IsNotNull(loaded);
        Assert.AreEqual(user.Id, loaded!.Id);
        Assert.AreEqual("armin", loaded.Username);
        Assert.AreEqual("HASH", loaded.PasswordHash);
    }

    [TestMethod]
    public void GetByUsername_ShouldFindUserCaseInsensitive()
    {
        var user = new User(Guid.NewGuid(), "Armin", "HASH", DateTime.UtcNow);

        var repo = CreateRepository();
        repo.Add(user);

        var loaded = repo.GetByUsername("armin");

        Assert.IsNotNull(loaded);
        Assert.AreEqual(user.Id, loaded!.Id);
    }

    [TestMethod]
    public void AddDuplicateUserId_ShouldThrowError()
    {
        var user = new User(Guid.NewGuid(), "armin", "HASH", DateTime.UtcNow);

        var repo = CreateRepository();
        repo.Add(user);

        Assert.ThrowsException<DuplicateUserException>(() => repo.Add(user));
    }

    [TestMethod]
    public void AddDuplicateUsername_ShouldThrowError()
    {
        var user1 = new User(Guid.NewGuid(), "armin", "HASH1", DateTime.UtcNow);
        var user2 = new User(Guid.NewGuid(), "Armin", "HASH2", DateTime.UtcNow);

        var repo = CreateRepository();
        repo.Add(user1);

        Assert.ThrowsException<DuplicateUserException>(() => repo.Add(user2));
    }
}