using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.UserWordsFileResponse;

public class UserWordsFileResponseDto
{
    public Guid UserId { get; set; }
    public List<WordDto.WordDto> Words { get; set; } = new();
}

public class UserWordsFileResponse
{
    public Guid UserId { get; set; }
    public List<Word> Words { get; set; } = new();
}