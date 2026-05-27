namespace AlwaysLearning.Models;

// Une thématique sélectionnable par l'utilisateur, associée à un terme de recherche Wikipédia.
public sealed record TopicTheme(string Id, string Label, string Emoji, string Query);

public static class ThemeCatalog
{
    public static readonly IReadOnlyList<TopicTheme> All = new[]
    {
        new TopicTheme("sciences", "Sciences", "🔬", "science"),
        new TopicTheme("histoire", "Histoire", "🏛️", "histoire"),
        new TopicTheme("geographie", "Géographie", "🗺️", "géographie"),
        new TopicTheme("nature", "Nature & animaux", "🌿", "animal"),
        new TopicTheme("espace", "Espace & astronomie", "🪐", "astronomie"),
        new TopicTheme("arts", "Arts & culture", "🎨", "art"),
        new TopicTheme("musique", "Musique", "🎵", "musique"),
        new TopicTheme("cinema", "Cinéma & séries", "🎬", "cinéma"),
        new TopicTheme("techno", "Technologie", "💻", "technologie"),
        new TopicTheme("philo", "Philosophie & idées", "🤔", "philosophie"),
        new TopicTheme("sport", "Sport", "⚽", "sport"),
        new TopicTheme("cuisine", "Cuisine & gastronomie", "🍳", "gastronomie"),
    };

    public static TopicTheme? ById(string id) => All.FirstOrDefault(t => t.Id == id);
}
