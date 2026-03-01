namespace SpellCheckingTool.Presentation.Http.Servers.Attributes;
    class HttpPut : HttpAttribute
    {
        public HttpPut(string path) : base("PUT", path)
        {
        }
    }
