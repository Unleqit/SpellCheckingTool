namespace SpellCheckingTool.Presentation.Http.Servers.Attributes;
    public class HttpPost : HttpAttribute
    {
        public HttpPost(string path) : base("POST", path)
        {
        }
    }
