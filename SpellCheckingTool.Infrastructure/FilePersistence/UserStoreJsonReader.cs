using Newtonsoft.Json;

namespace SpellCheckingTool.Infrastructure.UserPersistence;

public class UserStoreJsonReader
{
    public T ReadOrDefault<T>(string path, T defaultValue)
    {
        if (!File.Exists(path))
            return defaultValue;

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json) ?? defaultValue;
    }
}