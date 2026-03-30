using SpellCheckingTool.Application.Persistence.Exceptions;

namespace SpellCheckingTool.Application.Persistence;

public class FilePath
{
    public string Path { get; private set; }

    public FilePath(string filepath)
    {
        filepath = filepath.Trim().Replace(@"\", @"/");
        string fileDir = filepath.Contains("/") ? filepath.Substring(0, filepath.LastIndexOf("/")) : filepath;
        
        if (!Directory.Exists(fileDir))
            throw new InvalidFilePathException(fileDir);
        
        if (filepath.EndsWith("/"))
            throw new DirectoryPathProvidedException(filepath);

        Path = filepath;
    }
}
