using System.Runtime.InteropServices;

namespace SpellCheckingTool.Client
{
    public unsafe class SuggestionWindow : ISuggestionDisplay
    {
        const string consoleWindowTooSmallErrorMessage = "Please resize the console window to fit the suggestion buffer.";

        //-------public properties--------
        public int MaxSuggestionsToBeDisplayed { get; set; }
        int horizontalPaddingSz;
        public int HorizontalPaddingSz 
        {
            get => horizontalPaddingSz;
            set
            {
                horizontalPaddingSz = value < 0 ? 0 : value > 10 ? 10 : value;
                suggestionDisplayBuffer = new char[tree.metaData.wordBufferLength + 2 * horizontalPaddingSz];

                for (int i = 0; i < horizontalPaddingSz; ++i)
                    suggestionDisplayBuffer[i] = ' ';

                for (int i = suggestionDisplayBuffer.Length - 1 - horizontalPaddingSz; i < suggestionDisplayBuffer.Length; ++i)
                    suggestionDisplayBuffer[i] = ' ';
            }
        }
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
        
        ConsoleColor originalForeColor;
        ConsoleColor originalBackColor;
        int cursorLeft;
        int alreadyEnteredCharacterCount = 0;
        bool suggestionsShown = true;
        bool consoleTooSmallErrorMessageDisplayed = false;
        WordTree tree;
        SuggestionResult* currentSuggestions;

        //holds a single suggestion line with horizontalBufferSize padding on each side
        char[] suggestionDisplayBuffer;


        public SuggestionWindow(WordTree tree)
        {
            this.tree = tree;
            this.originalForeColor = Console.ForegroundColor;
            this.originalBackColor = Console.BackgroundColor;
            this.currentSuggestions = (SuggestionResult*)API.malloc(sizeof(SuggestionResult));
            this.suggestionDisplayBuffer = new char[tree.metaData.wordBufferLength + 2 * horizontalPaddingSz];

            //subscribe to handler to dynamically update longestWord property
            tree.wordTreeWordBufferLengthChangedEventHandler += ((object sender, int newLongestWord) =>
            {
                suggestionDisplayBuffer = new char[newLongestWord + 2 * horizontalPaddingSz];
            });
        }

        public void ShowSuggestionsForString(ref string input)
        {
            string lastWord = input.Substring(input.LastIndexOf(' ') + 1);

            switch (input[input.Length - 1])
            {
                default:
                    alreadyEnteredCharacterCount++;
                    cursorLeft++;
                    checkAndColorLastWord(lastWord);
                    hideSuggestions();
                    getSuggestions(lastWord);
                    showSuggestions();
                    break;

                case ' ':
                    alreadyEnteredCharacterCount = 0;
                    cursorLeft++;
                    hideSuggestions();
                    break;

                //pressing the backspace key in windows produces this character (ascii code 10, therefore (int)'\b' = 10)
                case '\b':
                    deleteLastChar(ref input);
                    break;

                //pressing the backspace key in linux produces this character (ascii code 127, hence we do an explicit cast of 127 as char => \u007f)
                case (char)127:
                    deleteLastChar(ref input);
                    break;
            }
        }

        void deleteLastChar(ref string input)
        {
            if (input.Length == 1)
            {
                input = "";
                return;
            }

            removeLastCharacterFromWord(ref input);

            string lastWord = input.Substring(input.LastIndexOf(' ') + 1);
            getSuggestions(lastWord);

            if (input.Length > 0)
                showSuggestions();
        }

        void checkAndColorLastWord(string lastWord)
        {
            bool contained = tree.Contains(lastWord.Trim().ToLower());
            Console.CursorLeft = cursorLeft - lastWord.Length;
            Console.BackgroundColor = contained ? ValidWordBackColor : InvalidWordBackColor;
            Console.ForegroundColor = contained ? ValidWordForeColor : InvalidWordForeColor;
            Console.Write(lastWord);
            Console.BackgroundColor = originalBackColor;
            Console.ForegroundColor = originalForeColor;
        }

        void removeLastCharacterFromWord(ref string input)
        {
            hideSuggestions();
            cursorLeft--;

            //the windows shell (cmd) seems to process backspace characters differently than a linux shell (tested with WSL/arch)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Write(" \b");
            }
            else
            {
                Console.CursorLeft--;
                Console.Write(" ");
            }

            input = input.Substring(0, input.Length - 2);
            alreadyEnteredCharacterCount = input.Length - (input.LastIndexOf(' ') + 1);
            checkAndColorLastWord(input.Substring(input.LastIndexOf(' ') + 1));
        }

        void getSuggestions(string input)
        {
            *currentSuggestions = tree.GetSuggestions(input, MaxSuggestionsToBeDisplayed, this.SuggestionAlgorithmMaxAllowedDistance);
            int suggestionResultCount = currentSuggestions->GetSuggestionCount();
            currentSuggestionCount = suggestionResultCount < MaxSuggestionsToBeDisplayed ? suggestionResultCount : currentSuggestionCount > MaxSuggestionsToBeDisplayed ? MaxSuggestionsToBeDisplayed : suggestionResultCount;
        }

        void showSuggestions()
        {
            suggestionsShown = true;
            int suggestionWindowWidth = tree.metaData.wordBufferLength + 2 * horizontalPaddingSz;
            int suggestionWindowHeight = currentSuggestionCount;

            //save the initial state of where the word is in the x direction of the console window and draw suggestion window at the start of the last character of said word
            int wordLeftInConsole = Console.CursorLeft;
            int wordTopInConsole = Console.CursorTop;

            char** suggestions = currentSuggestions->GetSuggestionArray();
            int* suggestionLengths = currentSuggestions->GetSuggestionLengths();


            //check if console is high enough to display suggestion window
            int currentLength = 0;
            if (Console.WindowHeight - Console.CursorTop >= suggestionWindowHeight + 1) //+1 because suggestion floating window gets shown below current line
            {
                for (int j = 0; j < suggestionWindowHeight; ++j)
                {
                    //remove error message, if it was previously shown
                    if (consoleTooSmallErrorMessageDisplayed)
                    {
                        Console.Write(new string(' ', consoleWindowTooSmallErrorMessage.Length));
                        Console.CursorLeft -= consoleWindowTooSmallErrorMessage.Length;
                        consoleTooSmallErrorMessageDisplayed = false;
                    }

                    Console.SetCursorPosition(wordLeftInConsole, Console.CursorTop + 1);
                    Console.BackgroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionBackColor : SuggestionBackColor;
                    Console.ForegroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionForeColor : SuggestionForeColor;

                    currentLength = suggestionLengths[j];


                    for (int i = 0; i < currentLength; ++i)
                        suggestionDisplayBuffer[i + horizontalPaddingSz] = suggestions[j][i];

                    for (int i = 0; i < suggestionDisplayBuffer.Length - 1 - horizontalPaddingSz - currentLength; ++i)
                        suggestionDisplayBuffer[i + horizontalPaddingSz + currentLength] = ' ';

                    Console.Write(suggestionDisplayBuffer, 0, suggestionDisplayBuffer.Length);
                }
            }
            else
            {
                //prompt the user to resize the console window (as this cannot be done programmatically in some systems)
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(consoleWindowTooSmallErrorMessage);
                Console.BackgroundColor = originalBackColor;
                Console.ForegroundColor = originalForeColor;
                consoleTooSmallErrorMessageDisplayed = true;
            }

            //restore cursor to old position and set old console colors
            Console.SetCursorPosition(wordLeftInConsole, wordTopInConsole);
            Console.ForegroundColor = originalForeColor;
            Console.BackgroundColor = originalBackColor;
        }

        public void hideSuggestions()
        {
            suggestionsShown = false;
            int suggestionWindowWidth = tree.metaData.wordBufferLength + 2 * horizontalPaddingSz;
            int suggestionWindowHeight = CurrentSuggestionCount;
            int cursorLeft = Console.CursorLeft;
            int wordTopInConsole = Console.CursorTop;

            //check if console is wide enough
            if (Console.WindowHeight - Console.CursorTop >= suggestionWindowHeight + 1) //+1 because suggestion floating window gets shown below current line
            {
                //remove error message, if it was previously shown
                if (consoleTooSmallErrorMessageDisplayed)
                {
                    Console.Write(new string(' ', consoleWindowTooSmallErrorMessage.Length));
                    Console.CursorLeft -= consoleWindowTooSmallErrorMessage.Length;
                    consoleTooSmallErrorMessageDisplayed = false;
                }

                //set colors
                Console.BackgroundColor = originalBackColor;
                Console.ForegroundColor = originalForeColor;

                //clear floating suggestions window
                for (int j = 0; j < suggestionWindowHeight; ++j)
                {
                    //set cursor pos
                    int startIndex = cursorLeft - 1 < 0 ? 0 : cursorLeft - 1;
                    Console.SetCursorPosition(startIndex, wordTopInConsole + j + 1);
                    Console.Write(new string(' ', suggestionWindowWidth + 2));
                }
            }
            else
            {
                consoleTooSmallErrorMessageDisplayed = true;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Please resize the window to allow the buffer to fit");
                Console.BackgroundColor = originalBackColor;
                Console.ForegroundColor = originalForeColor;
            }

            //restore cursor to old positions
            Console.SetCursorPosition(cursorLeft, wordTopInConsole);
        }

        public void autoCompleteCurrentlySelectedSuggestion(ref string input)
        {
            //this method gets called when the user presses the 'Enter' key, which automatically sets the 'Console.CursorLeft' property to 0, therefore we need to restore it to its previous value
            Console.CursorLeft = cursorLeft;
            hideSuggestions();
            Console.CursorLeft = cursorLeft - alreadyEnteredCharacterCount;

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
            cursorLeft += (selectedSuggestion.Length - alreadyEnteredCharacterCount);
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

            if (currentlySelectedLine > 0)
                currentlySelectedLine--;
            else
                currentlySelectedLine = currentSuggestionCount - 1;
            
            showSuggestions();
        }
    }
}
