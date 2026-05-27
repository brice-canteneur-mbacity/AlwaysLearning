using System.Net.Http.Json;
using System.Text.Json;
using AlwaysLearning.Models;

namespace AlwaysLearning.Services;

// Accès en lecture à l'API REST publique de Wikipédia en français.
// Toutes les requêtes partent du navigateur de l'utilisateur (CORS autorisé).
public sealed class WikipediaService
{
    private const string RestBase = "https://fr.wikipedia.org/api/rest_v1";
    private readonly HttpClient _http;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WikipediaService(HttpClient http) => _http = http;

    // Sélection éditoriale du jour : article à la une, image du jour, éphéméride.
    public async Task<FeaturedFeed?> GetFeaturedAsync(DateOnly date, CancellationToken ct = default)
    {
        var url = $"{RestBase}/feed/featured/{date.Year:0000}/{date.Month:00}/{date.Day:00}";
        return await _http.GetFromJsonAsync<FeaturedFeed>(url, JsonOptions, ct);
    }

    // Un article totalement aléatoire, pour le bouton « Découvrir un autre sujet ».
    public async Task<WikiArticle?> GetRandomAsync(CancellationToken ct = default)
    {
        var url = $"{RestBase}/page/random/summary";
        return await _http.GetFromJsonAsync<WikiArticle>(url, JsonOptions, ct);
    }
}
