using SpellCheckingTool.Application.UserStatsResponse;

public static class UserStatsResponseMapper
{
    public static UserStatsResponseDto ToStorage(UserStatsResponse domain)
    {
        return new UserStatsResponseDto
        {
            Stats = domain.Stats.Select((stat) => WordStatisticMapper.ToStorage(stat)).ToList(),
            UserId = domain.UserId
        };
    }

    public static UserStatsResponse ToDomain(UserStatsResponseDto dto)
    {
        return new UserStatsResponse()
        {
            Stats = dto.Stats.Select((stat) => WordStatisticMapper.ToDomain(stat)).ToList(),
            UserId = dto.UserId
        };
    }
}