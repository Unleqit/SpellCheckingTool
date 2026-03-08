namespace SpellCheckingTool.Presentation.ConsoleClient;
public class UserWordsFileResponseDto
{
    public Guid UserId { get; set; }
    public List<UserWordDto> Words { get; set; } = new();
}
