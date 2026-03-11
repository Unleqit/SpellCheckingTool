namespace SpellCheckingTool.Presentation.ConsoleClient;
public class AuthSession
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = "";
    public bool IsAuthenticated { get; set; }
}