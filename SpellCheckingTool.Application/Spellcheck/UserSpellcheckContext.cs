using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Spellcheck;

public sealed class UserSpellcheckContext
{
    public Guid? UserId { get; }
    public string? Username { get; }
    public WordTree Tree { get; set; }
    public ISpellcheckService SpellcheckService { get; set; }
    public ISpellcheckService ExecutableSpellcheckService { get; set; }

    public UserSettings Settings { get; }

    public IUserSettingsRepository SettingsRepository { get; }

    public bool IsAuthenticated => UserId.HasValue;

    public UserSpellcheckContext(
        Guid? userId,
        string? username,
        WordTree tree,
        ISpellcheckService spellcheckService,
        ISpellcheckService executableSpellcheckService,
        UserSettings settings,
        IUserSettingsRepository settingsRepository)
    {
        UserId = userId;
        Username = username;
        Tree = tree;
        SpellcheckService = spellcheckService;
        ExecutableSpellcheckService = executableSpellcheckService;
        Settings = settings;
        SettingsRepository = settingsRepository;
    }
}
