namespace SpellCheckingTool
{
    internal class WindowsExecutableParser : ExecutableParser
    {
        public override WordTree GetAllShellExecutables()
        {
            List<string> executablePaths = base.InvokeShellCommand("cmd", "/c \"where *.exe *.com *.bat *.cmd\"");
            IAlphabet alphabet = new ExecutableNameAlphabet();
            string[] executableNames = new string[executablePaths.Count];

            string current;
            for (int i = 0; i < executableNames.Length; ++i)
            {
                current = executablePaths[i];
                current = current.Substring(current.LastIndexOf('\\') + 1);
                current = current.Remove(current.IndexOf('.'));
                executableNames[i] = current;
            }

            Word[] words = Word.ParseWords(alphabet, executableNames);
            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            tree.Add(words);

            return tree;
        }
    }
}
