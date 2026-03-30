using SpellCheckingTool.Application.LoginResponse;
using SpellCheckingTool.Domain.Alphabet;

public static class LoginResponseMapper
{
    public static LoginResponseDto ToStorage(LoginResponse domain)
    {
        return new LoginResponseDto
        {
             UserId = domain.UserId,
             Username = domain.Username
        };
    }

    public static LoginResponse ToDomain(LoginResponseDto dto)
    {
        var alphabet = new UTF16Alphabet();
        return new LoginResponse()
        {
            Username = dto.Username,
            UserId = dto.UserId
        };
    }
}