namespace SpellCheckingTool.Presentation.Servers.Attributes;
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    abstract class HttpAttribute : Attribute
    {
        public string Path { get; }
        public string Method { get; }

        protected HttpAttribute(string method, string path)
        {
            Method = method;
            Path = path;
        }
    }
