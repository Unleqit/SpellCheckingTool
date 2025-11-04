namespace SpellCheckingTool
{
    class HttpDelete : HttpAttribute
    {
        public HttpDelete(string path) : base("DELETE", path)
        {
        }
    }
}
