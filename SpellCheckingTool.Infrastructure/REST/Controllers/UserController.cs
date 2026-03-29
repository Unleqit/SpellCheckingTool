using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Infrastructure.Http.Servers.Attributes;
using System.Net;

namespace SpellCheckingTool.Infrastructure.Http.Controllers;

public class UserController: BaseController
{
    UserService _service;

    public UserController(UserService service)
    {
        this._service = service;
    }

    [HttpPost("/api/v1/users/words/add")]
    public void AddWord(HttpListenerContext context, [FromBody] Guid userId, [FromBody] string word)
    {
        var result = _service.AddWord(userId, word);
        if (!result.Success)
        {
            WriteError(context, 404, result.ErrorMessage ?? "Could not add word.");
            return;
        }

        WriteJson(context, 200, new { success = true });
    }

    [HttpPost("/api/v1/users/words/delete")]
    public void DeleteWord(
    HttpListenerContext context,
    [FromBody] Guid userId,
    [FromBody] string word)
    {
        var result = _service.RemoveWord(userId, word);
        if (!result.Success)
        {
            WriteError(context, 404, result.ErrorMessage ?? "Could not delete word.");
            return;
        }

        WriteJson(context, 200, new { success = true });
    }

    [HttpPost("/api/v1/users/words/track")]
    public void TrackWordUsage(
        HttpListenerContext context,
        [FromBody] Guid userId,
        [FromBody] string word)
    {
        var result = _service.TrackWordUsage(userId, word);
        if (!result.Success)
        {
            WriteError(context, 404, result.ErrorMessage ?? "Could not track word usage.");
            return;
        }

        WriteJson(context, 200, new { success = true });
    }

    [HttpPost("/api/v1/users/words/file")]
    public void GetWordsFile(
        HttpListenerContext context,
        [FromBody] Guid userId)
    {
        var result = _service.GetCustomWords(userId);
        if (!result.Success || result.Value == null)
        {
            WriteError(context, 404, result.ErrorMessage ?? "No personal dictionary words found for this user.");
            return;
        }

        var ordered = result.Value
            .OrderBy(w => w.ToString(), StringComparer.OrdinalIgnoreCase);

        WriteJson(context, 200, new
        {
            userId,
            words = ordered.Select(w => new
            {
                word = w.ToString()
            })
        });
    }

    [HttpPost("/api/v1/users/words/stats")]
    public void GetStats(
        HttpListenerContext context,
        [FromBody] Guid userId)
    {
        var result = _service.GetStats(userId);
        if (!result.Success || result.Value == null)
        {
            WriteError(context, 404, result.ErrorMessage ?? "Could not get stats.");
            return;
        }

        var ordered = result.Value
            .OrderByDescending(s => s.UsageCount)
            .ThenBy(s => s.Word.ToString(), StringComparer.OrdinalIgnoreCase);

        WriteJson(context, 200, new
        {
            userId,
            stats = ordered.Select(s => new
            {
                word = s.Word.ToString(),
                usageCount = s.UsageCount,
                lastUsedAt = s.LastUsedAt
            })
        });
    }
}