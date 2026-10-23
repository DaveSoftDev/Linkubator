namespace Linkubator.Web.Models.AppDashboard;

public static class LinkCardViewModelFactory
{
    private static readonly string[] _accentClasses = ["blue", "violet", "green", "slate", "orange", "cyan", "rose", "amber"];

    private static readonly Dictionary<string, string> _collectionAccentMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["React"] = "blue",
        ["Trabajo"] = "amber",
        ["Diseño"] = "violet",
        ["Código"] = "green",
        ["Recursos"] = "cyan",
        ["Inspiración"] = "rose",
        ["Aprendizaje"] = "orange",
        ["Productos"] = "slate",
    };

    public static IReadOnlyList<LinkCardViewModel> CreateDefault()
    {
        return
        [
            new LinkCardViewModel
            {
                Title = "React Documentation",
                CollectionName = "React",
                Description = "Documentación oficial para construir interfaces con componentes, hooks y rendimiento.",
                Domain = "react.dev",
                AccentClass = ResolveAccentClass("React"),
                ImageUrl = null,
            },
            new LinkCardViewModel
            {
                Title = "Notion Workspace",
                CollectionName = "Trabajo",
                Description = "Espacio compartido para tareas, documentación y reuniones del equipo.",
                Domain = "notion.so",
                AccentClass = ResolveAccentClass("Trabajo"),
                ImageUrl = null,
            },
            new LinkCardViewModel
            {
                Title = "Figma Design Kit",
                CollectionName = "Diseño",
                Description = "Componentes visuales, tokens y sistema de diseño para la interfaz web.",
                Domain = "figma.com",
                AccentClass = ResolveAccentClass("Diseño"),
                ImageUrl = "https://biologue.plos.org/wp-content/uploads/sites/7/2023/03/690x320_BioSci_blog-header.png",
            },
            new LinkCardViewModel
            {
                Title = "GitHub Team Repo",
                CollectionName = "Código",
                Description = "Repositorio principal para el trabajo en equipo y la revisión de código.",
                Domain = "github.com",
                AccentClass = ResolveAccentClass("Código"),
                ImageUrl = null,
            },
            new LinkCardViewModel
            {
                Title = "Capacities",
                CollectionName = "Trabajo",
                Description = "Gestor de notas con enfoque en productividad y organización y permite implementar la metodología Zettelkasten.",
                Domain = "capacities.io",
                AccentClass = ResolveAccentClass("Trabajo"),
                ImageUrl = "https://capacities.io/thumbnail/thumbnail_index.png",
            },
        ];
    }

    private static string ResolveAccentClass(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            return "blue";
        }

        if (_collectionAccentMap.TryGetValue(collectionName.Trim(), out var mappedAccentClass))
        {
            return mappedAccentClass;
        }

        var hash = 0;
        foreach (var character in collectionName)
        {
            hash = (hash * 31) + character;
        }

        return _accentClasses[Math.Abs(hash) % _accentClasses.Length];
    }
}
