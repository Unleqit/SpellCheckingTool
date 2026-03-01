namespace SpellCheckingTool.Presentation.Http.Servers.Attributes;
    class HttpGet : HttpAttribute
    {
        public HttpGet(string path) : base("GET", path)
        {

        }
    }
