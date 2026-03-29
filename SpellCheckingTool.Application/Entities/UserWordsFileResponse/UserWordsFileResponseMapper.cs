using SpellCheckingTool.Application.UserWordsFileResponse;

public static class UserWordsFileResponseMapper
{
    public static UserWordsFileResponseDto ToStorage(UserWordsFileResponse domain)
    {
        return new UserWordsFileResponseDto
        {
            Words = domain.Words.Select((word) => WordMapper.ToStorage(word)).ToList(),
            UserId = domain.UserId
        };
    }

    public static UserWordsFileResponse ToDomain(UserWordsFileResponseDto dto)
    {
        return new UserWordsFileResponse()
        {
            Words = dto.Words.Select((word) => WordMapper.ToDomain(word)).ToList(),
            UserId = dto.UserId
        };
    }
}