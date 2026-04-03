using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public class WordService : IWordService
    {
        private readonly UserSpellcheckContext _context;
        private readonly ClientUserService _clientUserService;
        private readonly IUserSpellcheckContextFactory _factory;
        private readonly ISuggestionDisplay _suggestionDisplay;

        public WordService(
            UserSpellcheckContext context,
            ClientUserService clientUserService,
            IUserSpellcheckContextFactory factory,
            ISuggestionDisplay suggestionDisplay)
        {
            _context = context;
            _clientUserService = clientUserService;
            _factory = factory;
            _suggestionDisplay = suggestionDisplay;
        }

        public async Task<(bool success, string message)> AddWordAsync(string command)
        {
            string rawWord = command.Substring("/addword".Length).Trim();

            if (!_context.IsAuthenticated || _context.UserId == null)
                return (false, "You need to be logged in.");

            if (rawWord.Contains(' '))
                return (false, "Please enter exactly one word.");

            if (string.IsNullOrWhiteSpace(rawWord))
            {
                return (false, "Usage: /addword <word>");
            }

            if (!rawWord.All(char.IsLetter))
                return (false, "Invalid characters. Words must contain only letters.");

            string normalized = rawWord.ToLowerInvariant();

            Word word;
            try
            {
                word = new Word(_context.SpellcheckService.Alphabet, normalized);
            }
            catch (Exception ex)
            {
                return (false, $"Invalid word '{normalized}': {ex.Message}");
            }

            if (!_context.SpellcheckService.IsCorrect(word))
            {
                return (false, $"Invalid word '{normalized}'.");
            }

            bool persisted = await _clientUserService.Words.AddWord(_context.UserId.Value, normalized);
            if (!persisted)
                return (false, $"Word '{normalized}' was not saved.");

            try
            {
                RebuildActiveTreeAfterDictionaryChange();

            }
            catch (Exception ex)
            {
                await _clientUserService.Words.DeleteWord(_context.UserId.Value, normalized);
                RebuildActiveTreeAfterDictionaryChange();

                return (false, $"Invalid word '{normalized}': {ex.Message}");
            }

            return (true, $"Saved '{normalized}' to your personal dictionary.");
        }

        private void RebuildActiveTreeAfterDictionaryChange()
        {
            if (!_context.IsAuthenticated || _context.UserId == null || string.IsNullOrWhiteSpace(_context.Username))
                return;

            var refreshed = _factory.CreateForUser(_context.UserId.Value, _context.Username);

            _context.Tree = refreshed.Tree;
            _context.SpellcheckService = refreshed.SpellcheckService;

            _suggestionDisplay.HideSuggestions();
        }

        public async Task<(bool success, string message)> DeleteWordAsync(string command)
        {
            if (!_context.IsAuthenticated || _context.UserId == null)
                return (false, "You need to be logged in to delete a personal word.");

            string rawWord = command.Substring("/delword".Length).Trim();

            if (string.IsNullOrWhiteSpace(rawWord))
                return (false, "Usage: /delword <word>");

            if (rawWord.Contains(' '))
                return (false, "Please enter exactly one word.");

            string normalized = rawWord.ToLowerInvariant();

            bool deleted = await _clientUserService.Words.DeleteWord(_context.UserId.Value, normalized);
            if (!deleted)
                return (false, $"Word '{normalized}' was not found in your personal dictionary.");

            RebuildActiveTreeAfterDictionaryChange();

            bool stillValid;
            try
            {
                var word = new Word(_context.SpellcheckService.Alphabet, normalized);
                stillValid = _context.SpellcheckService.IsCorrect(word);
            }
            catch
            {
                stillValid = false;
            }

            string message = stillValid
                ? $"Deleted '{normalized}', but it is still valid via the default dictionary."
                : $"Deleted '{normalized}' from your personal dictionary.";

            return (true, message);
        }

        public async Task<IEnumerable<string>> GetWordsAsync()
        {
            if (!_context.IsAuthenticated || _context.UserId == null)
                return Array.Empty<string>();

            var words = await _clientUserService.Words.GetWords(_context.UserId.Value);


            return words
                .Select(w => w.ToString())
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
        } 
    }
}
