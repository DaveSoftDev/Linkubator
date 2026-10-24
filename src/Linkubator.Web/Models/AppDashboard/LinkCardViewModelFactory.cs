namespace Linkubator.Web.Models.AppDashboard;

public static class LinkCardViewModelFactory
{
    private static readonly string[] _accentClasses = ["blue", "violet", "green", "slate", "orange", "cyan", "rose", "amber", "yellow"];

    public static string ResolveAccent(string collectionName)
    {
        return ResolveAccentClass(collectionName);
    }

    public static IReadOnlyList<LinkCardViewModel> CreateDefault()
    {
        return
        [
            new LinkCardViewModel
            {
                Title = "Documentación de referencia",
                CollectionName = "Colección A",
                Description = "Material técnico para consultar y revisar dentro del flujo de trabajo.",
                Url = "https://example.com/a",
                ImageUrl = null,
                AccentClass = ResolveAccentClass("Colección A"),
            },
            new LinkCardViewModel
            {
                Title = "Notas de proyecto",
                CollectionName = "Colección B",
                Description = "Ideas, tareas y decisiones tomadas durante la evolución del trabajo.",
                Url = "https://example.com/b",
                AccentClass = ResolveAccentClass("Colección B"),
                ImageUrl = null,
            },
            new LinkCardViewModel
            {
                Title = "Diseño visual",
                CollectionName = "Colección C",
                Description = "Recursos visuales y referencias para mantener consistencia en la interfaz.",
                Url = "https://example.com/c",
                AccentClass = ResolveAccentClass("Colección C"),
                ImageUrl = "https://example.com/image.jpg",
            },
        ];
    }

    private static string ResolveAccentClass(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            return _accentClasses[0];
        }

        string normalizedName = collectionName.Trim();

        int hash = 0;
        foreach (var character in normalizedName)
        {
            hash = (hash * 31) + char.ToUpperInvariant(character);
        }

        return _accentClasses[Math.Abs(hash) % _accentClasses.Length];
    }
}
