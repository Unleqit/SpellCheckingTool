namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public interface IWordService
    {
        Task<(bool success, string message)> AddWordAsync(string command);
        Task<(bool success, string message)> DeleteWordAsync(string rawWord);
        Task<IEnumerable<string>> GetWordsAsync();
        }
}
