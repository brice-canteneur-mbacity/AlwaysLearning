using System.Text.Json;
using Microsoft.JSInterop;
using AlwaysLearning.Models;

namespace AlwaysLearning.Services;

// Gère le contenu « du jour » : récupération + mise en cache locale (localStorage),
// l'historique des jours consultés et la série de jours consécutifs (streak).
public sealed class DailyService
{
    private readonly WikipediaService _wiki;
    private readonly IJSRuntime _js;

    private const string ThemeKeyPrefix = "al:theme:";
    private const string HistoryKey = "al:history";
    private const string StreakKey = "al:streak";
    private const string LastOpenKey = "al:lastOpen";
    private const string FavoritesKey = "al:favorites";

    public DailyService(WikipediaService wiki, IJSRuntime js)
    {
        _wiki = wiki;
        _js = js;
    }

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    // Thème du jour : article aléatoire mis en cache, stable pour la journée.
    public async Task<Article?> GetThemeAsync(DateOnly date, CancellationToken ct = default)
    {
        var cached = await GetCachedThemeAsync(date);
        if (cached is not null)
            return cached;

        return await FetchAndCacheAsync(date, ct);
    }

    // Tire un nouveau thème au hasard et remplace celui en cache pour aujourd'hui.
    public async Task<Article?> RerollThemeAsync(DateOnly date, CancellationToken ct = default)
        => await FetchAndCacheAsync(date, ct);

    private async Task<Article?> FetchAndCacheAsync(DateOnly date, CancellationToken ct)
    {
        var article = await _wiki.GetRandomArticleAsync(ct);
        if (article is not null)
        {
            await SetItemAsync(ThemeKeyPrefix + Key(date), JsonSerializer.Serialize(article, WikipediaService.JsonOptions));
            await AddToHistoryAsync(date, article.Title);
        }
        return article;
    }

    private async Task<Article?> GetCachedThemeAsync(DateOnly date)
    {
        var json = await GetItemAsync(ThemeKeyPrefix + Key(date));
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<Article>(json, WikipediaService.JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    // --- Historique ---

    public async Task<List<HistoryEntry>> GetHistoryAsync()
    {
        var json = await GetItemAsync(HistoryKey);
        if (string.IsNullOrEmpty(json))
            return new();
        try
        {
            var list = JsonSerializer.Deserialize<List<HistoryEntry>>(json, WikipediaService.JsonOptions);
            return list ?? new();
        }
        catch
        {
            return new();
        }
    }

    private async Task AddToHistoryAsync(DateOnly date, string? title)
    {
        var history = await GetHistoryAsync();
        var key = Key(date);
        history.RemoveAll(h => h.Date == key);
        history.Insert(0, new HistoryEntry { Date = key, Title = title ?? "Sujet du jour" });
        if (history.Count > 60)
            history = history.Take(60).ToList();
        await SetItemAsync(HistoryKey, JsonSerializer.Serialize(history, WikipediaService.JsonOptions));
    }

    // --- Favoris ---

    public async Task<List<SavedArticle>> GetFavoritesAsync()
    {
        var json = await GetItemAsync(FavoritesKey);
        if (string.IsNullOrEmpty(json))
            return new();
        try
        {
            return JsonSerializer.Deserialize<List<SavedArticle>>(json, WikipediaService.JsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> IsFavoriteAsync(string key)
    {
        var favorites = await GetFavoritesAsync();
        return favorites.Any(f => f.Key == key);
    }

    // Ajoute ou retire le favori et renvoie son nouvel état (true = désormais en favori).
    public async Task<bool> ToggleFavoriteAsync(SavedArticle article)
    {
        var favorites = await GetFavoritesAsync();
        var existing = favorites.FirstOrDefault(f => f.Key == article.Key);
        bool isFavorite;
        if (existing is not null)
        {
            favorites.Remove(existing);
            isFavorite = false;
        }
        else
        {
            favorites.Insert(0, article);
            isFavorite = true;
        }
        await SetItemAsync(FavoritesKey, JsonSerializer.Serialize(favorites, WikipediaService.JsonOptions));
        return isFavorite;
    }

    // --- Série de jours consécutifs ---

    // Met à jour la série au lancement et renvoie le nombre de jours d'affilée.
    public async Task<int> RegisterVisitAsync()
    {
        var today = Key(Today);
        var lastOpen = await GetItemAsync(LastOpenKey);
        var streak = ParseInt(await GetItemAsync(StreakKey));

        if (lastOpen == today)
            return Math.Max(streak, 1);

        var yesterday = Key(Today.AddDays(-1));
        streak = lastOpen == yesterday ? streak + 1 : 1;

        await SetItemAsync(StreakKey, streak.ToString());
        await SetItemAsync(LastOpenKey, today);
        return streak;
    }

    // --- Helpers localStorage ---

    private async Task<string?> GetItemAsync(string key)
        => await _js.InvokeAsync<string?>("localStorage.getItem", key);

    private async Task SetItemAsync(string key, string value)
        => await _js.InvokeVoidAsync("localStorage.setItem", key, value);

    private static string Key(DateOnly date) => date.ToString("yyyy-MM-dd");

    private static int ParseInt(string? s) => int.TryParse(s, out var v) ? v : 0;
}

public sealed class HistoryEntry
{
    public string Date { get; set; } = "";
    public string Title { get; set; } = "";
}
