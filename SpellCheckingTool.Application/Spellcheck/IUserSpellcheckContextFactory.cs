using SpellCheckingTool.Application.Spellcheck;

public interface IUserSpellcheckContextFactory
{
    UserSpellcheckContext CreateAnonymous();
    UserSpellcheckContext CreateForUser(Guid userId, string username);
}