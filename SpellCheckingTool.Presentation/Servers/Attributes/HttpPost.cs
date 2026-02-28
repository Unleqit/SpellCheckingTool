namespace SpellCheckingTool.Presentation.Servers.Attributes;
    class HttpPost : HttpAttribute
    {
        public HttpPost(string path) : base("POST", path)
        {
        }
    }
