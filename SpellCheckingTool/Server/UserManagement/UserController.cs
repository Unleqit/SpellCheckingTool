using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SpellCheckingTool;

namespace SpellCheckingTool
{
    public static class UserController
    {
        // TODO: replace `LatinAlphabet` with the one we actually use
        private static readonly IAlphabet _alphabet = new LatinAlphabet();

        private static readonly FileUserStore _store =
            new(Path.Combine(AppContext.BaseDirectory, "data"), _alphabet);

        private static readonly UserService _service = new(_store, _store);

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

        //Authentication

        [HttpPost("/api/v1/users/register")]
        public static void Register(
           HttpListenerContext context,
           [FromBody] string username,
           [FromBody] string hashedPassword)
        {
            var result = _service.Register(username, hashedPassword);
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
            var result = _service.Login(username, hashedPassword);
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

        //Adding words
        [HttpPost("/api/v1/users/words/add")]
        public static void AddWord(
            HttpListenerContext context,
            [FromBody] Guid userId,
            [FromBody] string word)
        {
            var result = _service.AddWord(userId, word);
            if (!result.Success)
            {
                WriteError(context, 404, result.ErrorMessage ?? "Could not add word.");
                return;
            }

            WriteJson(context, 200, new { success = true });
        }

        //Show words file (raw view)
        [HttpPost("/api/v1/users/words/file")]
        public static void GetWordsFile(
            HttpListenerContext context,
            [FromBody] Guid userId)
        {
            var all = _store.GetAllWordStatsRaw();

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

        //Show statistics (sorted view)
        [HttpPost("/api/v1/users/words/stats")]
        public static void GetStats(
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
}
