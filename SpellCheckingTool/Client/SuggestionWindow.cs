using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Client
{
    public unsafe class SuggestionWindow : ISuggestionDisplay
    {
        //-------public properties--------
        public int MaxSuggestionsToBeDisplayed { get; set; }
        public int HorizontalPaddingSz { get; set; }
        public ConsoleColor ValidWordForeColor { get; set; }
        public ConsoleColor ValidWordBackColor { get; set; }
        public ConsoleColor InvalidWordForeColor { get; set; }
        public ConsoleColor InvalidWordBackColor { get; set; }
        public ConsoleColor SuggestionForeColor { get; set; }
        public ConsoleColor SuggestionBackColor { get; set; }
        public ConsoleColor CurrentlySelectedSuggestionForeColor { get; set; }
        public ConsoleColor CurrentlySelectedSuggestionBackColor { get; set; }
        
        int currentSuggestionCount;
        public int CurrentSuggestionCount { get => currentSuggestionCount; }
        
        int suggestionAlgorithmMaxAllowedDistance;
        public int SuggestionAlgorithmMaxAllowedDistance
        {
            get => suggestionAlgorithmMaxAllowedDistance;
            set => suggestionAlgorithmMaxAllowedDistance = value < 0 ? 0 : value;
        }

        int currentlySelectedLine;
        public int CurrentlySelectedLine 
        {
            get => currentlySelectedLine;
            set => currentlySelectedLine = value < 0 ? 0 : value > currentSuggestionCount ? currentSuggestionCount : value;
        }
        public string CurrentlySelectedSuggestion
        {
            get
            {
                if (currentlySelectedLine >= currentSuggestions->GetSuggestionCount() || currentlySelectedLine < 0)
                    return "";

                string tmp = "";
                char** suggestions = currentSuggestions->GetSuggestionArray();
                int* suggestionLengths = currentSuggestions->GetSuggestionLengths();

                for (int i = 0; i < suggestionLengths[currentlySelectedLine]; ++i)
                    tmp += suggestions[currentlySelectedLine][i];

                return tmp;
            }
        }


        //-------private properties--------
        
        int longestWord;
        ConsoleColor originalForeColor;
        ConsoleColor originalBackColor;
        int oldCursorLeft;
        int alreadyEnteredCharacterCount = 0;
        bool suggestionsShown = true;
        WordTree tree;
        bool cycle;
        SuggestionResult* currentSuggestions;


        public SuggestionWindow(WordTree tree)
        {
            this.tree = tree;
            this.longestWord = tree.metaData.wordBufferLength;
            this.originalForeColor = Console.ForegroundColor;
            this.originalBackColor = Console.BackgroundColor;
            this.currentSuggestions = (SuggestionResult*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(SuggestionResult)) : API.linux_malloc(sizeof(SuggestionResult)));

            //subscribe to handler to dynamically update longestWord property
            tree.wordTreeWordBufferLengthChangedEventHandler += ((object sender, int newLongestWord) => longestWord = newLongestWord);
        }

        public void ShowSuggestionsForString(ref string input)
        {
            string lastWord = input.Substring(input.LastIndexOf(' ') + 1);
            string[] suggestions;

            switch (input[input.Length - 1])
            {
                default:
                    alreadyEnteredCharacterCount++;

                    //check if input is contained in the tree and color the last word accordingly
                    checkAndColorLastWord(lastWord);
                    hideSuggestions();
                    getSuggestions(lastWord);
                    showSuggestions();
                    oldCursorLeft++;
                    break;
                case ' ':
                    alreadyEnteredCharacterCount = 0;
                    oldCursorLeft++;
                    hideSuggestions();
                    break;
                case '\b':
                    if (input.Length == 1)
                    {
                        input = "";
                        break;
                    }

                    //rewrite previous word in normal color
                    removeLastCharacterFromWord(ref input);

                    //show suggestions again for last word
                    lastWord = input.Substring(input.LastIndexOf(' ') + 1);
                    getSuggestions(lastWord);
                    showSuggestions();
                    break;
            }
        }

        void checkAndColorLastWord(string lastWord)
        {
            bool contained = tree.Contains(lastWord.Trim().ToLower());
            Console.CursorLeft = Console.CursorLeft - lastWord.Length;
            Console.BackgroundColor = contained ? ValidWordBackColor : InvalidWordBackColor;
            Console.ForegroundColor = contained ? ValidWordForeColor : InvalidWordForeColor;
            Console.Write(lastWord);
            Console.BackgroundColor = originalBackColor;
            Console.ForegroundColor = originalForeColor;
        }

        void removeLastCharacterFromWord(ref string input)
        {
            //clear suggestions
            Console.CursorLeft++;
            hideSuggestions();
            Console.CursorLeft--;
            oldCursorLeft--;
            
            //clear last character shown in console
            Console.Write(" \b");

            //remove last character form input buffer
            input = input.Substring(0, input.Length - 2);

            //set alreadyEnteredCharacterCount
            alreadyEnteredCharacterCount = input.Length - (input.LastIndexOf(' ') + 1);
            checkAndColorLastWord(input.Substring(input.LastIndexOf(' ') + 1));
        }

        void getSuggestions(string input)
        {
            *currentSuggestions = tree.GetSuggestions(input, MaxSuggestionsToBeDisplayed, this.SuggestionAlgorithmMaxAllowedDistance);
            int _currentSuggestionCount = currentSuggestions->GetSuggestionCount();
            currentSuggestionCount = _currentSuggestionCount < MaxSuggestionsToBeDisplayed ? _currentSuggestionCount : currentSuggestionCount > MaxSuggestionsToBeDisplayed ? MaxSuggestionsToBeDisplayed : _currentSuggestionCount;
        }

        void showSuggestions()
        {
            suggestionsShown = true;
            int suggestionWindowWidth = longestWord + 2 * HorizontalPaddingSz;
            int suggestionWindowHeight = currentSuggestionCount;

            //save the initial state of where the word is in the x direction of the console window and draw suggestion window at the start of the last character of said word
            //TODO: clean this up
            int wordLeftInConsole = Console.CursorLeft;
            int wordTopInConsole = Console.CursorTop;

            char** suggestions = currentSuggestions->GetSuggestionArray();
            int* suggestionLengths = currentSuggestions->GetSuggestionLengths();

            //check if console is high enough to display suggestion window
            if (Console.WindowHeight - Console.CursorTop >= suggestionWindowHeight + 1) //+1 because suggestion floating window gets shown below current line
            {
                for (int j = 0; j < suggestionWindowHeight; ++j)
                {
                    Console.SetCursorPosition(wordLeftInConsole, Console.CursorTop + 1);
                    Console.BackgroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionBackColor : SuggestionBackColor;
                    Console.ForegroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionForeColor : SuggestionForeColor;
                    Console.Write(new string(' ', HorizontalPaddingSz));

                    for (int i = 0; i < suggestionLengths[j]; ++i)
                        Console.Write(suggestions[j][i]);

                   Console.Write(new string(' ', suggestionWindowWidth - (HorizontalPaddingSz + suggestionLengths[j])));
                }
            }
            else
            {
                //TODO
            }

            //restore cursor to old position and set old console colors
            Console.SetCursorPosition(wordLeftInConsole, wordTopInConsole);
            Console.ForegroundColor = originalForeColor;
            Console.BackgroundColor = originalBackColor;
        }

        public void hideSuggestions()
        {
            suggestionsShown = false;
            int suggestionWindowWidth = longestWord + 2 * HorizontalPaddingSz;
            int suggestionWindowHeight = CurrentSuggestionCount;
            int oldCursorLeft = Console.CursorLeft;
            int wordTopInConsole = Console.CursorTop;

            //check if console is wide enough
            if (Console.WindowHeight - Console.CursorTop >= suggestionWindowHeight + 1) //+1 because suggestion floating window gets shown below current line
            {
                //set colors
                Console.BackgroundColor = originalBackColor;
                Console.ForegroundColor = originalForeColor;

                //clear floating suggestions window
                for (int j = 0; j < suggestionWindowHeight; ++j)
                {
                    //set cursor pos
                    Console.SetCursorPosition(oldCursorLeft - 1, wordTopInConsole + j + 1);
                    Console.Write(new string(' ', suggestionWindowWidth + 1));
                }
            }
            else
            {
                //we'll handle that later...
            }

            //restore cursor to old positions
            Console.SetCursorPosition(oldCursorLeft, wordTopInConsole);
        }

        public void autoCompleteCurrentlySelectedSuggestion(ref string input)
        {
            Console.CursorLeft = oldCursorLeft;
            hideSuggestions();
            Console.CursorLeft = oldCursorLeft - alreadyEnteredCharacterCount;
            Console.ForegroundColor = ValidWordForeColor;
            Console.BackgroundColor = ValidWordBackColor;
            string selectedSuggestion = CurrentlySelectedSuggestion;

            //example: "avocad" entered, completed with suggestion "avoid" -> 'd' of "avocad" needs to be cleared!
            if (selectedSuggestion.Length < alreadyEnteredCharacterCount)
            {
                Console.CursorLeft += selectedSuggestion.Length;
                Console.Write(new string(' ', alreadyEnteredCharacterCount - selectedSuggestion.Length));
                Console.CursorLeft -= selectedSuggestion.Length + 1;
            }

            Console.Write(selectedSuggestion);
            Console.ForegroundColor = originalForeColor;
            Console.BackgroundColor = originalBackColor;
            oldCursorLeft += (selectedSuggestion.Length - alreadyEnteredCharacterCount);
            currentlySelectedLine = 0;
            alreadyEnteredCharacterCount = 0;
            input = input.Substring(0, input.LastIndexOf(' ') + 1) + selectedSuggestion;
        }

        public void selectNextSuggestion()
        {
            if (!suggestionsShown)
                return;

            hideSuggestions();
            currentlySelectedLine = (currentlySelectedLine + 1) % currentSuggestionCount;
            showSuggestions();

        }

        public void selectPreviousSuggestion()
        {
            if (!suggestionsShown)
                return;
            hideSuggestions();

            //this has wasted too much of my time already
            if (currentlySelectedLine > 0)
                currentlySelectedLine--;
            else
                currentlySelectedLine = currentSuggestionCount - 1;
            
            showSuggestions();
        }
    }
}
