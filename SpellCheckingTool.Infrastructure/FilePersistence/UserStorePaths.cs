namespace SpellCheckingTool.Infrastructure.UserPersistence;

public class UserStorePaths
{
    public string UsersFilePath { get; }
    public string WordStatsFilePath { get; }
    public string CustomDictionaryFilePath { get; }

    public UserStorePaths(string baseDirectory)
    {
        Directory.CreateDirectory(baseDirectory);

        UsersFilePath = Path.Combine(baseDirectory, "users.json");
        WordStatsFilePath = Path.Combine(baseDirectory, "wordstats.json");
        CustomDictionaryFilePath = Path.Combine(baseDirectory, "userdictionary.json");
    }
}