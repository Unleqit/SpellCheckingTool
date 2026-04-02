using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Application.Settings;
using System.Text;

namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public static class StatsFormatter
    {
        public static string FormatStats(IReadOnlyList<WordStatistic> stats, UserSettings userSettings)
        {
            if (stats == null || stats.Count == 0)
                return "No stats found.";

            int maxDisplayedStats = userSettings.MaxDisplayedStats;
            var topStats = stats
                .OrderByDescending(s => s.UsageCount)
                .ThenBy(s => s.Word.ToString(), StringComparer.OrdinalIgnoreCase)
                .Take(maxDisplayedStats)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Top used words:");
            sb.AppendLine("────────────────────────────────────────────");
            sb.AppendLine($"{"Word",-15} {"Count",-7} {"Last Used"}");
            sb.AppendLine("────────────────────────────────────────────");

            foreach (var item in topStats)
            {
                sb.AppendLine($"{item.Word,-15} {item.UsageCount,-7} {item.LastUsedAt:yyyy-MM-dd HH:mm:ss}");
            }

            sb.AppendLine("────────────────────────────────────────────");
            sb.AppendLine($"Showing top {topStats.Count} of {stats.Count} tracked words.");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
