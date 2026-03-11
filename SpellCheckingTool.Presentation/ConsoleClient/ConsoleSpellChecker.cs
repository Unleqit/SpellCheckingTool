using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleSpellChecker
{
    private readonly ShellProcessManager _processManager;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly SuggestionUseCase _suggestionUseCase;

    private const string WelcomeMessage = "Type text and press space to check words.";

    private void UpdateSuggestions(string input)
    {
        
        var viewModel = _suggestionUseCase.Execute(input);
        viewModel.Offset = _processManager.CurrentShellOffset;

        _suggestionDisplay.Show(viewModel);
    }

    public ConsoleSpellChecker(
        ISpellcheckService spellcheckService,
        SuggestionUseCase suggestionUseCase,
        ShellProcessManager processManager,
        ISuggestionDisplay suggestionWindow)
    {
        _suggestionUseCase = suggestionUseCase;
        _processManager = processManager;
        _suggestionDisplay = suggestionWindow;
    }    

    public void Run()
    {
        Console.WriteLine(WelcomeMessage);
        string input = "";

        Console.Write(_processManager.GetCurrentConsolePrompt());

        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            char c = keyInfo.KeyChar;

            switch (keyInfo.Key)
            {
                default:
                    //ignore control chars like Ctrl, Alt, Super, ...
                    if (char.IsControl(c))
                        break;

                    input += c;
                    Console.Write(c);
                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Backspace:
                case (ConsoleKey)127:
                    if (input == "")
                        break;

                    input = input.Substring(0, input.Length - 1);
                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Enter:
                    if (_suggestionDisplay.IsCurrentlyVisible())
                        _suggestionDisplay.AutoCompleteCurrentlySelectedSuggestion(ref input);
                    else
                    {
                        Console.WriteLine();
                        _processManager.SendInput(input);

                        Console.Write(_processManager.GetCurrentConsolePrompt());

                        input = "";
                    }
                    break;

                case ConsoleKey.UpArrow:
                    _suggestionDisplay.SelectPreviousSuggestion();
                    break;

                case ConsoleKey.DownArrow:
                    _suggestionDisplay.SelectNextSuggestion();
                    break;

                case ConsoleKey.Escape:
                    _suggestionDisplay.HideSuggestions();
                    break;
            }
        }
    }
}
