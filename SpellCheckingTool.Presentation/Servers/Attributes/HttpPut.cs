namespace SpellCheckingTool.Presentation.Servers.Attributes;
    class HttpPut : HttpAttribute
    {
        public HttpPut(string path) : base("PUT", path)
        {
        }
    }
