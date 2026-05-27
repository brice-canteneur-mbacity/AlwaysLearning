using System.Text.Json.Serialization;

namespace AlwaysLearning.Models;

// Réponse de l'endpoint REST /feed/featured/{yyyy}/{mm}/{dd} de Wikipédia.
public sealed class FeaturedFeed
{
    [JsonPropertyName("tfa")]
    public WikiArticle? FeaturedArticle { get; set; }

    [JsonPropertyName("image")]
    public PictureOfTheDay? Image { get; set; }

    [JsonPropertyName("onthisday")]
    public List<OnThisDayEvent>? OnThisDay { get; set; }
}

// Résumé d'un article (tfa, random/summary, pages liées...).
public sealed class WikiArticle
{
    [JsonPropertyName("titles")]
    public ArticleTitles? Titles { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("extract")]
    public string? Extract { get; set; }

    [JsonPropertyName("thumbnail")]
    public WikiImage? Thumbnail { get; set; }

    [JsonPropertyName("originalimage")]
    public WikiImage? OriginalImage { get; set; }

    [JsonPropertyName("content_urls")]
    public ContentUrls? ContentUrls { get; set; }

    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    public string DisplayTitle => Titles?.Normalized ?? Title ?? "Sujet du jour";

    public string? PageUrl => ContentUrls?.Mobile?.Page ?? ContentUrls?.Desktop?.Page;
}

public sealed class ArticleTitles
{
    [JsonPropertyName("normalized")]
    public string? Normalized { get; set; }
}

public sealed class WikiImage
{
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public sealed class ContentUrls
{
    [JsonPropertyName("desktop")]
    public UrlSet? Desktop { get; set; }

    [JsonPropertyName("mobile")]
    public UrlSet? Mobile { get; set; }
}

public sealed class UrlSet
{
    [JsonPropertyName("page")]
    public string? Page { get; set; }
}

public sealed class PictureOfTheDay
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("thumbnail")]
    public WikiImage? Thumbnail { get; set; }

    [JsonPropertyName("image")]
    public WikiImage? Image { get; set; }

    [JsonPropertyName("description")]
    public DescriptionText? Description { get; set; }
}

public sealed class DescriptionText
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public sealed class OnThisDayEvent
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("pages")]
    public List<WikiArticle>? Pages { get; set; }
}
