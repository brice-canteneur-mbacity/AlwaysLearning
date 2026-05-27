using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlwaysLearning.Models;

namespace AlwaysLearning.Services;

// Accès en lecture à l'API MediaWiki de Wikipédia en français (CORS via origin=*).
public sealed class WikipediaService
{
    private const string ApiBase = "https://fr.wikipedia.org/w/api.php";
    private readonly HttpClient _http;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WikipediaService(HttpClient http) => _http = http;

    // Pioche quelques articles au hasard et renvoie le plus fourni (évite les ébauches).
    public async Task<Article?> GetRandomArticleAsync(CancellationToken ct = default)
    {
        var url = ApiBase
            + "?action=query&format=json&origin=*"
            + "&generator=random&grnnamespace=0&grnlimit=4"
            + "&prop=extracts|pageimages|info|description"
            + "&explaintext=1&exsectionformat=wiki&exlimit=max"
            + "&inprop=url&piprop=thumbnail|original&pithumbsize=800";

        var response = await _http.GetFromJsonAsync<ActionResponse>(url, JsonOptions, ct);
        var pages = response?.Query?.Pages?.Values;
        if (pages is null)
            return null;

        var best = pages
            .Where(p => !string.IsNullOrWhiteSpace(p.Title) && !string.IsNullOrWhiteSpace(p.Extract))
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
    }

    private sealed class ImageInfo
    {
        [JsonPropertyName("source")] public string? Source { get; set; }
    }
}
