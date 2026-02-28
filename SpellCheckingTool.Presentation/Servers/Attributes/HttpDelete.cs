namespace SpellCheckingTool.Presentation.Servers.Attributes;
    class HttpDelete : HttpAttribute
    {
        public HttpDelete(string path) : base("DELETE", path)
        {
        }
    }
