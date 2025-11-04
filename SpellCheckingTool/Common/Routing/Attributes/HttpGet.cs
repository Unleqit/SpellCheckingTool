namespace SpellCheckingTool
{
    class HttpGet : HttpAttribute
    {
        public HttpGet(string path) : base("GET", path)
        {

        }
    }
}
