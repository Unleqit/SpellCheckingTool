namespace SpellCheckingTool.Presentation.ConsoleClient;
public class UserStatsResponseDto
{
    public Guid UserId { get; set; }
    public List<UserWordStatDto> Stats { get; set; } = new();
}