using Newtonsoft.Json;
using SpellCheckingTool.Application.UserService;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Presentation.Servers.Attributes;
using System.Net;

namespace SpellCheckingTool.Presentation.Controller;

public static class UserController
{
    // Dependencies are injected once from Program (composition root).
    private static FileUserStore? _store;
    private static UserService? _service;

    /// <summary>
    /// Called once during startup from Program.cs to inject dependencies.
    /// </summary>
    public static void Configure(FileUserStore store, UserService service)
    {
        _store = store;
        _service = service;
    }

    private static void EnsureConfigured()
    {
        if (_store == null || _service == null)
            throw new InvalidOperationException("UserController is not configured. Call UserController.Configure(...) during startup.");
    }

    private static void WriteJson(HttpListenerContext context, int statusCode, object payload)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        using var writer = new StreamWriter(context.Response.OutputStream);
        var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
        writer.Write(json);
    }

    private static void WriteError(HttpListenerContext context, int statusCode, string message)
    {
        WriteJson(context, statusCode, new { error = message });
    }

    // Authentication

    [HttpPost("/api/v1/users/register")]
    public static void Register(
        HttpListenerContext context,
        [FromBody] string username,
        [FromBody] string hashedPassword)
    {
        EnsureConfigured();

        var result = _service!.Register(username, hashedPassword);
        if (!result.Success || result.Value == null)
        {
            WriteError(context, 400, result.ErrorMessage ?? "Registration failed.");
            return;
        }

        WriteJson(context, 200, new
        {
            userId = result.Value.Id,
            username = result.Value.Username,
            createdAt = result.Value.CreatedAt
        });
    }

    [HttpPost("/api/v1/users/login")]
    public static void Login(
        HttpListenerContext context,
        [FromBody] string username,
        [FromBody] string hashedPassword)
    {
        EnsureConfigured();

        var result = _service!.Login(username, hashedPassword);
        if (!result.Success || result.Value == null)
        {
            WriteError(context, 401, result.ErrorMessage ?? "Login failed.");
            return;
        }

        WriteJson(context, 200, new
        {
            userId = result.Value.Id,
            username = result.Value.Username
        });
    }

    // Adding words

    [HttpPost("/api/v1/users/words/add")]
    public static void AddWord(
        HttpListenerContext context,
        [FromBody] Guid userId,
        [FromBody] string word)
    {
        EnsureConfigured();

        var result = _service!.AddWord(userId, word);
        if (!result.Success)
        {
            WriteError(context, 404, result.ErrorMessage ?? "Could not add word.");
            return;
        }

        WriteJson(context, 200, new { success = true });
    }

    // Show words file (raw view)
    // NOTE: This still reaches into FileUserStore directly.
    // We'll clean this later by moving it behind an Application method.
    [HttpPost("/api/v1/users/words/file")]
    public static void GetWordsFile(
        HttpListenerContext context,
        [FromBody] Guid userId)
    {
        EnsureConfigured();

        var all = _store!.GetAllWordStatsRaw();

        if (!all.TryGetValue(userId, out var wordsForUser))
        {
            WriteError(context, 404, "No words found for this user.");
            return;
        }

        WriteJson(context, 200, new
        {
            userId,
            words = wordsForUser.Values.Select(w => new
            {
                word = w.Word.ToString(),
                usageCount = w.UsageCount,
                lastUsedAt = w.LastUsedAt
            })
        });
    }

    // Show statistics (sorted view)

    [HttpPost("/api/v1/users/words/stats")]
    public static void GetStats(
        HttpListenerContext context,
        [FromBody] Guid userId)
    {
        EnsureConfigured();

        var result = _service!.GetStats(userId);
        if (!result.Success || result.Value == null)
        {
            WriteError(context, 404, result.ErrorMessage ?? "Could not get stats.");
            return;
        }

        var ordered = result.Value
            .OrderByDescending(s => s.UsageCount)
            .ThenBy(s => s.Word);

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