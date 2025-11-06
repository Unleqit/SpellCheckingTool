using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SpellCheckingTool
{
    public unsafe class Client
    {
        public static void StartClient(int port)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));

            string path = Path.Combine(projectRoot, @"TestProject\Resources\wordFile.wdb");

            FilePath filePath = new FilePath(path);
            WordTree tree = new FilePersistenceService().Load(filePath);

            Console.WriteLine("Type text and press space to check words.");

            StringBuilder currentWord = new StringBuilder();
            List<string> previousWords = new List<string>();

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                char c = keyInfo.KeyChar;

                //exit
                //if (c == '!')
                    //break;

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (currentWord.Length > 0)
                    {
                        // remove last character from the word
                        currentWord.Remove(currentWord.Length - 1, 1);

                        // move cursor one step back, overwrite char with space, move back again
                        Console.Write("\b \b");
                    }

                    // If Backspace is pressed after a completed word, restore (retype) the previous wordt
                    else if (previousWords.Count > 0)
                    {
                        string lastWord = previousWords.Last();
                        previousWords.RemoveAt(previousWords.Count - 1);
                        for (int i = 0; i < lastWord.Length + 1; i++)
                            Console.Write("\b \b");

                        currentWord.Append(lastWord);
                        Console.Write(currentWord);
                    }
                        continue;
                }

                // check spelling
                if (c == ' ' || keyInfo.Key == ConsoleKey.Enter)
                {
                    if (currentWord.Length == 0)
                        continue;

                    string originalWord = currentWord.ToString();
                    string lowercaseWord = originalWord.ToLower();

                    bool exists = tree.Contains(lowercaseWord);

                    Console.SetCursorPosition(Console.CursorLeft - currentWord.Length, Console.CursorTop);

                    if (exists)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write(originalWord + " ");
                    Console.ForegroundColor = ConsoleColor.White;

                    previousWords.Add(originalWord);
                    currentWord.Clear();
                    continue;
                }


                currentWord.Append(c);
                Console.Write(c);
            }
        }
    }
}