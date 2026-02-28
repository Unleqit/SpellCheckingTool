namespace SpellCheckingTool.Application.PersistenceService;
    public class FilePath
    {
        public string Path { get; private set; }

        public FilePath(string filepath)
        {
            filepath = filepath.Trim().Replace(@"\", @"/");
            //get the subdirectory this filepath points to
            string fileDir = filepath.Contains("/") ? filepath.Substring(0, filepath.LastIndexOf("/")) : filepath;
            //check if the directory exists
            if (!Directory.Exists(fileDir))
                throw new Exception("Invalid filepath: " + fileDir);
            //check that not a directory was supplied
            if (filepath.EndsWith("/"))
                throw new Exception("Specified filepath is a directory");

            this.Path = filepath;
        }
    }
