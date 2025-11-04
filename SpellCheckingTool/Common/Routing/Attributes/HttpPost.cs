namespace SpellCheckingTool
{
    class HttpPost : HttpAttribute
    {
        public HttpPost(string path) : base("POST", path)
        {
        }
    }
}
