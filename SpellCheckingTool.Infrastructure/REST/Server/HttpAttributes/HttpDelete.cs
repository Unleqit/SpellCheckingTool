namespace SpellCheckingTool.Infrastructure.Http.Servers.Attributes;
    public class HttpDelete : HttpAttribute
    {
        public HttpDelete(string path) : base("DELETE", path)
        {
        }
    }
