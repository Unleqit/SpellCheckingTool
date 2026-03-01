namespace SpellCheckingTool.Presentation.Http.Servers.Attributes;
    class HttpPost : HttpAttribute
    {
        public HttpPost(string path) : base("POST", path)
        {
        }
    }
