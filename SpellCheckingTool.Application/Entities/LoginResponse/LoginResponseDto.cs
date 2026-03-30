namespace SpellCheckingTool.Application.LoginResponse;

public class LoginResponse
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Username { get; set; } = "";
}

public class LoginResponseDto
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Username { get; set; } = "";
}