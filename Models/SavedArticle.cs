namespace AlwaysLearning.Models;

// Article mémorisé dans les favoris (stocké tel quel pour pouvoir l'afficher sans rappeler l'API).
public sealed class SavedArticle
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Extract { get; set; }
    public string? Url { get; set; }
    public string? Thumbnail { get; set; }

    // Identité d'un favori : l'URL si dispo, sinon le titre.
    public string Key => string.IsNullOrEmpty(Url) ? Title : Url!;
}
