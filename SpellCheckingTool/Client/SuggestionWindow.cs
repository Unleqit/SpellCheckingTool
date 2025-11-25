using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Client
{
    public class SuggestionWindow : ISuggestionDisplay
    {
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
        public int CurrentSuggestionCount { get => currentSuggestionCount; }
        public int SuggestionAlgorithmMaxAllowedDistance
        {
            get => suggestionAlgorithmMaxAllowedDistance;
            //the provided distance should not be negative
            set => suggestionAlgorithmMaxAllowedDistance = value < 0 ? 0 : value;
        }
        public int CurrentlySelectedLine 
        {
            get => currentlySelectedLine;
            //the currently selected line should not exceed the bounds of the suggestion window
            set => currentlySelectedLine = value < 0 ? 0 : value > currentSuggestionCount ? currentSuggestionCount : value;
        }
        public string CurrentlySelectedSuggestion { get => currentlySelectedLine < currentSuggestions.Length ? currentSuggestions[currentlySelectedLine] : ""; }

        int suggestionAlgorithmMaxAllowedDistance;
        int currentlySelectedLine;
        int currentSuggestionCount;
        string[] currentSuggestions;
        int longestWord;
        ConsoleColor originalForeColor;
        ConsoleColor originalBackColor;
        int oldCursorLeft;
        int alreadyEnteredCharacterCount = 0;
        bool suggestionsShown = true;
        WordTree tree;
        bool cycle;
        bool previousWordAutoCompleted;

        public SuggestionWindow(WordTree tree)
        {
            this.tree = tree;
            this.longestWord = tree.metaData.wordBufferLength;
            this.originalForeColor = Console.ForegroundColor;
            this.originalBackColor = Console.BackgroundColor;
            

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
                    suggestions = getSuggestions(lastWord);
                    showSuggestions(suggestions);
                    oldCursorLeft++;
                    break;
                case ' ':
                    alreadyEnteredCharacterCount = 0;
                    oldCursorLeft++;
                    hideSuggestions();
                    break;
                case '\b':
                    if (input.Length == 1)
                        break;

                    //rewrite previous word in normal color
                    removeLastCharacterFromWord(ref input);

                    //show suggestions again for last word
                    lastWord = input.Substring(input.LastIndexOf(' ') + 1);
                    suggestions = getSuggestions(lastWord);
                    showSuggestions(suggestions);
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

            //rewrite last word without match highlighting
        //    int startIndexOfLastWord = input.LastIndexOf(' ') + 1;
            checkAndColorLastWord(input.Substring(input.LastIndexOf(' ') + 1));


        /*    Console.CursorLeft = startIndexOfLastWord;
            Console.ForegroundColor = originalForeColor;
            Console.BackgroundColor = originalBackColor;

            //write only last word, not the entire sequence stored in input
            Console.Write(input.Substring(input.LastIndexOf(' ') + 1)); */
        }

        string[] getSuggestions(string input)
        {
            var sw = Stopwatch.StartNew();
            string[] result = tree.GetSuggestions(input, MaxSuggestionsToBeDisplayed, this.SuggestionAlgorithmMaxAllowedDistance).GetSuggestionArray();
            currentSuggestions = result;
            currentSuggestionCount = result.Length < MaxSuggestionsToBeDisplayed ? result.Length : result.Length > MaxSuggestionsToBeDisplayed ? MaxSuggestionsToBeDisplayed : result.Length;
            sw.Stop();
            long l = sw.ElapsedMilliseconds;
            
            return result;
        }

        void showSuggestions(string[] suggestions)
        {
            int suggestionWindowWidth = longestWord + 2 * HorizontalPaddingSz;
            int suggestionWindowHeight = CurrentSuggestionCount;

            //save the initial state of where the word is in the x direction of the console window and draw suggestion window at the start of the last character of said word
            //TODO: clean this up
            int wordLeftInConsole = Console.CursorLeft - (cycle ? 1 : 0);
            int wordTopInConsole = Console.CursorTop;

            //check if console is high enough to display suggestion window
            if (Console.WindowHeight - Console.CursorTop >= suggestionWindowHeight + 1) //+1 because suggestion floating window gets shown below current line
            {
                for (int j = 0; j < suggestionWindowHeight; ++j)
                {
                    Console.SetCursorPosition(wordLeftInConsole, Console.CursorTop + 1);
                    Console.BackgroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionBackColor : SuggestionBackColor;
                    Console.ForegroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionForeColor : SuggestionForeColor;
                    Console.Write(new string(' ', HorizontalPaddingSz));
                    Console.Write(suggestions[j]);
                   Console.Write(new string(' ', suggestionWindowWidth - (HorizontalPaddingSz + suggestions[j].Length)));
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
            previousWordAutoCompleted = true;
        }

        public void selectNextSuggestion()
        {
            if (!suggestionsShown)
                return;
            hideSuggestions();
            currentlySelectedLine = (currentlySelectedLine + 1) % currentSuggestionCount;
            showSuggestions(currentSuggestions);

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
            
            showSuggestions(currentSuggestions);
        }
    }
}
