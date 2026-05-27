using System.Text;
using System.Text.RegularExpressions;

namespace AlwaysLearning.Models;

// Un thème Wikipédia avec son texte complet, à partir duquel on dérive les niveaux de lecture.
public sealed class Article
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public string FullText { get; set; } = "";
}

public sealed record ReadingLevel(string Label, string Text, int Words, int Minutes);

// Découpe le texte d'un article en trois profondeurs de lecture cumulatives.
public static class ReadingLevels
{
    private const int WordsPerMinute = 200;
    private const int CourtWords = 500;
    private const int MoyenWords = 1500;

    public static IReadOnlyList<ReadingLevel> Build(string fullText)
    {
        var paragraphs = Regex.Split(fullText.Trim(), @"\n{2,}")
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        var court = Truncate(paragraphs, CourtWords);
        var moyen = Truncate(paragraphs, MoyenWords);
        var full = string.Join("\n\n", paragraphs);

        var levels = new List<ReadingLevel> { Make("Court", court) };
        if (moyen.Length > court.Length)
            levels.Add(Make("Moyen", moyen));
        if (full.Length > moyen.Length)
            levels.Add(Make("Long", full));

        // Article court : un seul niveau, qu'on renomme pour ne pas afficher « Court » seul.
        if (levels.Count == 1)
            levels[0] = levels[0] with { Label = "Article" };

        return levels;
    }

    private static string Truncate(List<string> paragraphs, int wordBudget)
    {
        var sb = new StringBuilder();
        var words = 0;
        foreach (var p in paragraphs)
        {
            sb.Append(p).Append("\n\n");
            words += CountWords(p);
            if (words >= wordBudget)
                break;
        }
        return sb.ToString().Trim();
    }

    private static ReadingLevel Make(string label, string text)
    {
        var words = CountWords(text);
        var minutes = Math.Max(1, (int)Math.Round(words / (double)WordsPerMinute));
        return new ReadingLevel(label, text, words, minutes);
    }

    private static int CountWords(string s)
        => s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
}
