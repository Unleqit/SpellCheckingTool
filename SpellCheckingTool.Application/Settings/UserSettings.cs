using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Application.Settings
{
    public class UserSettings
    {
        public int MaxSuggestions { get; set; }
        public int MaxDistance { get; set; }

        public int HorizontalPadding { get; set; }

        public int MaxDisplayedStats { get; set; }

        public ConsoleColor ValidWordForeColor { get; set; }
        public ConsoleColor ValidWordBackColor { get; set; }

        public ConsoleColor InvalidWordForeColor { get; set; }
        public ConsoleColor InvalidWordBackColor { get; set; }

        public ConsoleColor SuggestionForeColor { get; set; }
        public ConsoleColor SuggestionBackColor { get; set; } 

        public ConsoleColor SelectedSuggestionForeColor { get; set; } 
        public ConsoleColor SelectedSuggestionBackColor { get; set; }

        public bool EnableCapitalizationInInput { get; set; }
        public bool AdjustCapitalizationInSuggestions { get; set; }

        public static UserSettings Default => new UserSettings
        {
            MaxSuggestions = 5,
            MaxDistance = 3,
            HorizontalPadding = 3,
            MaxDisplayedStats = 5,
            ValidWordForeColor = ConsoleColor.Green,
            ValidWordBackColor = Console.BackgroundColor,
            InvalidWordForeColor = ConsoleColor.Red,
            InvalidWordBackColor = Console.BackgroundColor,
            SuggestionForeColor = ConsoleColor.White,
            SuggestionBackColor = ConsoleColor.Red,
            SelectedSuggestionForeColor = ConsoleColor.Cyan,
            SelectedSuggestionBackColor = ConsoleColor.Yellow,
            EnableCapitalizationInInput = true,
            AdjustCapitalizationInSuggestions = true
        };
    }
}
