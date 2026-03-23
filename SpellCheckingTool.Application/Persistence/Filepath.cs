using SpellCheckingTool.Application.Persistence.Exceptions;

namespace SpellCheckingTool.Application.Persistence;
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
                throw new InvalidFilePathException(fileDir);
        //check that not a directory was supplied
            if (filepath.EndsWith("/"))
                throw new DirectoryPathProvidedException(filepath);

        Path = filepath;
        }
    }
