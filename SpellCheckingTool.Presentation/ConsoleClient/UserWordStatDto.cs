namespace SpellCheckingTool.Presentation.ConsoleClient;
public class UserWordStatDto
{
    public string Word { get; set; }
    public int UsageCount { get; set; }
    public DateTime LastUsedAt { get; set; }
}
