namespace SpellCheckingTool.Presentation.ConsoleClient;
public class UserWordsFileResponseDto
{
    public Guid UserId { get; set; }
    public List<UserDictionaryWordDto> Words { get; set; } = new();
}
