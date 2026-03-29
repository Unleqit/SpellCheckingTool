namespace SpellCheckingTool.Infrastructure.Http.Servers.Attributes;
    public class HttpPost : HttpAttribute
    {
        public HttpPost(string path) : base("POST", path)
        {
        }
    }
