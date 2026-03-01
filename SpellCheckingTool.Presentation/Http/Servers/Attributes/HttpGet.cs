namespace SpellCheckingTool.Presentation.Http.Servers.Attributes;
    public class HttpGet : HttpAttribute
    {
        public HttpGet(string path) : base("GET", path)
        {

        }
    }
