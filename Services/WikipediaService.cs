using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlwaysLearning.Models;

namespace AlwaysLearning.Services;

// Accès en lecture à l'API MediaWiki de Wikipédia en français (CORS via origin=*).
public sealed class WikipediaService
{
    private const string ApiBase = "https://fr.wikipedia.org/w/api.php";

    // Propriétés communes demandées pour chaque page candidate.
    private const string PageProps =
        "&prop=extracts|pageimages|info|description|pageprops"
        + "&explaintext=1&exsectionformat=wiki&exlimit=max"
        + "&inprop=url&piprop=thumbnail|original&pithumbsize=800&ppprop=disambiguation";

    private readonly HttpClient _http;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WikipediaService(HttpClient http) => _http = http;

    // Renvoie un article. Si des thématiques sont fournies, on en pioche une et on
    // tire un article au hasard parmi ses résultats ; sinon (ou en cas d'échec) on
    // retombe sur un tirage totalement aléatoire. Les pages d'homonymie sont écartées.
    public async Task<Article?> GetRandomArticleAsync(
        IReadOnlyList<string>? themeQueries = null, CancellationToken ct = default)
    {
        if (themeQueries is { Count: > 0 })
        {
            var query = themeQueries[Random.Shared.Next(themeQueries.Count)];
            var themed = await TryFetchAsync(BuildSearchUrl(query), ct);
            if (themed is not null)
                return themed;
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var article = await TryFetchAsync(BuildRandomUrl(), ct);
            if (article is not null)
                return article;
        }
        return null;
    }

    private static string BuildRandomUrl()
        => ApiBase + "?action=query&format=json&origin=*"
                   + "&generator=random&grnnamespace=0&grnlimit=6" + PageProps;

    private static string BuildSearchUrl(string query)
        => ApiBase + "?action=query&format=json&origin=*"
                   + "&generator=search&gsrnamespace=0&gsrsort=random&gsrlimit=6"
                   + "&gsrsearch=" + Uri.EscapeDataString(query) + PageProps;

    private async Task<Article?> TryFetchAsync(string url, CancellationToken ct)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ActionResponse>(url, JsonOptions, ct);
            var pages = response?.Query?.Pages?.Values;
            if (pages is null)
                return null;

            var best = pages
                .Where(p => !string.IsNullOrWhiteSpace(p.Title)
                            && !string.IsNullOrWhiteSpace(p.Extract)
                            && !IsDisambiguation(p))
                .OrderByDescending(p => p.Extract!.Length)
                .FirstOrDefault();

            if (best is null)
                return null;

            return new Article
            {
                Title = best.Title!,
                Description = best.Description,
                Url = best.FullUrl,
                ImageUrl = best.Original?.Source ?? best.Thumbnail?.Source,
                FullText = best.Extract!,
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsDisambiguation(ActionPage page)
        => (page.PageProps is not null && page.PageProps.ContainsKey("disambiguation"))
           || (page.Description?.Contains("homonymie", StringComparison.OrdinalIgnoreCase) ?? false);

    private sealed class ActionResponse
    {
        [JsonPropertyName("query")] public ActionQuery? Query { get; set; }
    }

    private sealed class ActionQuery
    {
        [JsonPropertyName("pages")] public Dictionary<string, ActionPage>? Pages { get; set; }
    }

    private sealed class ActionPage
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("extract")] public string? Extract { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("fullurl")] public string? FullUrl { get; set; }
        [JsonPropertyName("thumbnail")] public ImageInfo? Thumbnail { get; set; }
        [JsonPropertyName("original")] public ImageInfo? Original { get; set; }
        [JsonPropertyName("pageprops")] public Dictionary<string, JsonElement>? PageProps { get; set; }
    }

    private sealed class ImageInfo
    {
        [JsonPropertyName("source")] public string? Source { get; set; }
    }
}
