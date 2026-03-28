using Newtonsoft.Json;

namespace SpellCheckingTool.Infrastructure.FilePersistence
{
    public class UserStoreJsonWriter
    {
        public void Write<T>(string path, T data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
