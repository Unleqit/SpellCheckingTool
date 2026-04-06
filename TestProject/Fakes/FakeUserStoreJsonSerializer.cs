using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.UserPersistence;

namespace TestProject.Fakes;

public class FakeUserStoreJsonSerializer : IUserStoreJsonSerializer
{
    private readonly Dictionary<string, object> _storage = new();

    public T ReadOrDefault<T>(string path, T defaultValue)
    {
        if (!_storage.TryGetValue(path, out var value))
            return defaultValue;

        return value is T typedValue ? typedValue : defaultValue;
    }

    public void Write<T>(string path, T data)
    {
        _storage[path] = data!;
    }
}