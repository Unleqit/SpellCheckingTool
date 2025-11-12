using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SpellCheckingTool.Client
{
    public class Client
    {
        public static void StartClient(int port)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));
            string path = Path.Combine(projectRoot, @"TestProject\Resources\wordFile.wdb");

            FilePath filePath = new FilePath(path);
            WordTree tree = new WordTree();
            tree = new FilePersistenceService(tree).Load(filePath);

            // start cmd process
            var processManager = new ProcessManager();
            processManager.Start();

            // start spell checking
            var consoleSpellChecker = new ConsoleSpellChecker(tree, processManager);
            consoleSpellChecker.Run();
        }
    }
}      