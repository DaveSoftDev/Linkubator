using LinkDto = Linkubator.Application.Dtos.Models.LinkDto;

namespace Linkubator.Web.Models.AppDashboard;

public static class LinkCardViewModelFactory
{
    private static readonly string[] _accentClasses = ["blue", "violet", "green", "slate", "orange", "cyan", "rose", "amber", "yellow"];

    /// <summary>
    /// Determina la clase de acento visual asociada al nombre de colección.
    /// </summary>
    public static string ResolveAccent(string collectionName)
    {
        return ResolveAccentClass(collectionName);
    }

    /// <summary>
    /// Crea un modelo para presentar de forma genérica que un recurso privado no está disponible.
    /// </summary>
    public static LinkCardViewModel Create(LinkDto link)
    {
        string collectionName = link.CollectionName ?? string.Empty;
        return new LinkCardViewModel
        {
            Id = link.Id,
            IsPublic = link.IsPublic,
            Title = string.IsNullOrWhiteSpace(link.Title) ? link.UrlOriginal : link.Title,
            CollectionName = collectionName,
            Description = link.Description ?? string.Empty,
            ImageUrl = link.Image,
            AccentClass = ResolveAccent(collectionName),
            Url = link.UrlOriginal,
        };
    }

    /// <summary>
    /// Crea tarjetas de ejemplo para mostrar el estado de demostración.
    /// </summary>
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

    /// <summary>
    /// Selecciona de forma determinista una clase de acento para el nombre de colección.
    /// </summary>
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

        int index = (int)(Math.Abs((long)hash) % _accentClasses.Length);
        return _accentClasses[index];
    }
}
