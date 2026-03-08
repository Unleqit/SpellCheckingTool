using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Spellcheck;

public sealed class UserSpellcheckContext
{
    public Guid? UserId { get; }
    public string? Username { get; }
    public WordTree Tree { get; }
    public ISpellcheckService SpellcheckService { get; }

    public bool IsAuthenticated => UserId.HasValue;

    public UserSpellcheckContext(
        Guid? userId,
        string? username,
        WordTree tree,
        ISpellcheckService spellcheckService)
    {
        UserId = userId;
        Username = username;
        Tree = tree;
        SpellcheckService = spellcheckService;
    }
}
