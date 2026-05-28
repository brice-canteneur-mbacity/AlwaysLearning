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

// Découpe le texte d'un article en trois volets distincts : Court (intro),
// Détaillé (sections suivantes), Complet (le reste). Chaque volet contient
// uniquement sa portion propre, pour être affichés dans un accordéon.
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

        var (courtParas, rest1) = TakeUntil(paragraphs, CourtWords);
        var courtWords = CountAll(courtParas);
        var (detailParas, completParas) = TakeUntil(rest1, Math.Max(1, MoyenWords - courtWords));

        var panels = new List<ReadingLevel>();
        if (courtParas.Count > 0)
            panels.Add(Make("Court", string.Join("\n\n", courtParas)));
        if (detailParas.Count > 0)
            panels.Add(Make("Détaillé", string.Join("\n\n", detailParas)));
        if (completParas.Count > 0)
            panels.Add(Make("Complet", string.Join("\n\n", completParas)));
        if (panels.Count == 0)
            panels.Add(Make("Article", fullText));

        return panels;
    }

    // Avale des paragraphes jusqu'à atteindre le budget de mots, puis renvoie la suite.
    private static (List<string> taken, List<string> rest) TakeUntil(List<string> paragraphs, int wordBudget)
    {
        var taken = new List<string>();
        var words = 0;
        var i = 0;
        while (i < paragraphs.Count)
        {
            taken.Add(paragraphs[i]);
            words += CountWords(paragraphs[i]);
            i++;
            if (words >= wordBudget)
                break;
        }
        return (taken, paragraphs.Skip(i).ToList());
    }

    private static ReadingLevel Make(string label, string text)
    {
        var words = CountWords(text);
        var minutes = Math.Max(1, (int)Math.Round(words / (double)WordsPerMinute));
        return new ReadingLevel(label, text, words, minutes);
    }

    private static int CountWords(string s)
        => s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    private static int CountAll(IEnumerable<string> paragraphs)
        => paragraphs.Sum(CountWords);
}
