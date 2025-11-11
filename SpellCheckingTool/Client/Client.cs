using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace SpellCheckingTool
{
    public class Client
    {
        public static void StartClient(int port)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));

            string path = Path.Combine(projectRoot, @"TestProject\Resources\wordFile.wdb");

            FilePath filePath = new FilePath(path);
            WordTree tree = new WordTree(new LatinAlphabet());
            tree = new FilePersistenceService(tree).Load(filePath);

            //starting CMD process
            var p = new Process();
            p.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            p.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                    Console.WriteLine(e.Data);
            };

            p.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Data);
                    Console.ResetColor();
                }
            };


            p.Start();
            p.StandardInput.AutoFlush = true;
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            Console.WriteLine("Type text and press space to check words.");

            StringBuilder currentWord = new StringBuilder();
            string input = "";

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                char c = keyInfo.KeyChar;

                //exit
                if (keyInfo.Key == ConsoleKey.Escape)
                    break;

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    p.StandardInput.WriteLine(input);
                    input = "";
                    currentWord.Clear();
                    continue;
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        // move cursor one step back, overwrite char with space, move back again
                        Console.Write("\b \b");

                        if (currentWord.Length > 0)
                        {
                            // remove last character from the current word
                            currentWord.Remove(currentWord.Length - 1, 1);
                        }

                        else
                        {
                            // if Backspace is pressed after a completed word, restore the previous word
                            int lastSpace = input.LastIndexOf(' ');
                            if (lastSpace >= 0)
                            {
                                string lastWord = input.Substring(lastSpace + 1);
                                currentWord.Clear();
                                currentWord.Append(lastWord);
                            }
                        }
                    }
                    continue;
                }

                input += c;

                // check spelling
                if (c == ' ')
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

                    currentWord.Clear();
                    continue;
                }


                currentWord.Append(c);
                Console.Write(c);
            }
        }
    }
}