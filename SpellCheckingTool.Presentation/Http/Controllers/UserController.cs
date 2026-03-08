using Newtonsoft.Json;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Presentation.Http.Servers.Attributes;
using System.Net;

namespace SpellCheckingTool.Presentation.Http.Controllers;

public static class UserController
{
    private static UserService? _service;

    /// <summary>
    /// Called once during startup from Program.cs to inject dependencies.
    /// </summary>
    public static void Configure(UserService service)
    {
        _service = service;
    }

    private static void EnsureConfigured()
    {
        if (_service == null)
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

    [HttpPost("/api/v1/users/words/track")]
    public static void TrackWordUsage(
        HttpListenerContext context,
        [FromBody] Guid userId,
        [FromBody] string word)
    {
        EnsureConfigured();

        var result = _service!.TrackWordUsage(userId, word);
        if (!result.Success)
        {
            WriteError(context, 404, result.ErrorMessage ?? "Could not track word usage.");
            return;
        }

        WriteJson(context, 200, new { success = true });
    }

    [HttpPost("/api/v1/users/words/file")]
    public static void GetWordsFile(
        HttpListenerContext context,
        [FromBody] Guid userId)
    {
        EnsureConfigured();

        var result = _service!.GetCustomWords(userId);
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