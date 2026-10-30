using System.Reflection;
using DomainEntities = Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

[Trait("Category", "EntitiesSurfaces")]
public class EntityPublicSurfaceTests
{
    [Fact]
    public void EntityPropertiesDoNotExposePublicSettersForSensitiveMembers()
    {
        AssertSensitiveProperties(typeof(DomainEntities.User), ["Id", "Email", "Alias", "Password", "SecurityStamp"]);
        AssertSensitiveProperties(typeof(DomainEntities.Collection), ["Id", "UserId", "IsPublic"]);
        AssertSensitiveProperties(typeof(DomainEntities.Link), ["Id", "UserId", "CollectionId", "UrlOriginal", "UrlNormalized", "IsPublic"]);
    }

    private static void AssertSensitiveProperties(Type entityType, string[] names)
    {
        foreach (string name in names)
        {
            PropertyInfo? property = entityType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(property);
            Assert.False(HasPublicSetter(property), $"{entityType.Name}.{name} must not expose a public setter.");
        }
    }

    [Fact]
    public void ReflectionTestDetectsPublicSetter()
    {
        PropertyInfo? property = typeof(PublicSetterEntity).GetProperty(nameof(PublicSetterEntity.UrlOriginal));
        Assert.NotNull(property);
        Assert.True(HasPublicSetter(property));
        Assert.False(HasPublicSetter(null));
    }

    [Fact]
    public void HasPublicSetter_ReturnsFalse_WhenPropertyHasNoSetter()
    {
        PropertyInfo? property = typeof(NoSetterEntity).GetProperty(nameof(NoSetterEntity.ReadOnlyProp));

        Assert.False(HasPublicSetter(property));
    }

    private class NoSetterEntity
    {
        public string ReadOnlyProp { get; } = string.Empty;
    }

    [Fact]
    public void HasPublicSetter_ReturnsFalse_WhenPropertyHasPrivateSetter()
    {
        PropertyInfo? property = typeof(PrivateSetterEntity).GetProperty(nameof(PrivateSetterEntity.PropWithPrivateSetter));

        Assert.False(HasPublicSetter(property));
    }

    private class PrivateSetterEntity
    {
        public string PropWithPrivateSetter { get; private set; } = string.Empty;
    }

    [Fact]
    public void PublicSetterEntity_PropertyCanBeGetAndSet()
    {
        PublicSetterEntity entity = new PublicSetterEntity();

        string original = entity.UrlOriginal;
        entity.UrlOriginal = "test";

        Assert.Equal(string.Empty, original);
        Assert.Equal("test", entity.UrlOriginal);
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
