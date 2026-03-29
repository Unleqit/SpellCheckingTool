namespace SpellCheckingTool.Infrastructure.Http.Servers.Attributes;
    public class HttpPut : HttpAttribute
    {
        public HttpPut(string path) : base("PUT", path)
        {
        }
    }
