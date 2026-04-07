using Newtonsoft.Json;
using SpellCheckingTool.Infrastructure.FilePersistence;

namespace SpellCheckingTool.Infrastructure.UserPersistence;

public class UserStoreJsonSerializer : IUserStoreJsonSerializer
{
    public T ReadOrDefault<T>(string path, T defaultValue)
    {
        if (!File.Exists(path))
            return defaultValue;

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json) ?? defaultValue;
    }

    public void Write<T>(string path, T data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}