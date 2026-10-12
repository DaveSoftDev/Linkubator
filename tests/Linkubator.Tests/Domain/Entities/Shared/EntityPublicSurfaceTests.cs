using DomainEntities = Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

[Trait("Category", "EntitiesSurfaces")]
public class EntityPublicSurfaceTests
{
    [Fact]
    public void EntityPropertiesDoNotExposePublicSettersForSensitiveMembers()
    {
        string[] sensitiveProperties = ["UrlOriginal", "UrlNormalized", "IsPublic", "CollectionId", "UserId"];

        foreach (Type entityType in new[] { typeof(DomainEntities.User), typeof(DomainEntities.Collection), typeof(DomainEntities.Link) })
        {
            foreach (string propertyName in sensitiveProperties)
            {
                System.Reflection.PropertyInfo? property = entityType.GetProperty(
                    propertyName,
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (property is not null)
                {
                    Assert.False(HasPublicSetter(property), $"{entityType.Name}.{propertyName} must not expose a public setter.");
                }
            }
        }
    }

    [Fact]
    public void ReflectionTestDetectsPublicSetter()
    {
        var property = typeof(PublicSetterEntity).GetProperty(nameof(PublicSetterEntity.UrlOriginal));
        Assert.NotNull(property);
        Assert.True(HasPublicSetter(property));
        Assert.False(HasPublicSetter(null));
    }

    private static bool HasPublicSetter(System.Reflection.PropertyInfo? property)
    {
        return property?.SetMethod is { IsPublic: true };
    }

    private sealed class PublicSetterEntity
    {
        public string UrlOriginal { get; set; } = string.Empty;
    }
}
