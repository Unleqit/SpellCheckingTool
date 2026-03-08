namespace SpellCheckingTool.Presentation.ConsoleClient;
public class UserStatsResponseDto
{
    public Guid UserId { get; set; }
    public List<UserWordDto> Stats { get; set; } = new();
}