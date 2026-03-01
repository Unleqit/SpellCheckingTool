namespace SpellCheckingTool.Presentation.Http.Servers.Attributes;
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class HttpAttribute : Attribute
    {
        public string Path { get; }
        public string Method { get; }

        protected HttpAttribute(string method, string path)
        {
            Method = method;
            Path = path;
        }
    }
