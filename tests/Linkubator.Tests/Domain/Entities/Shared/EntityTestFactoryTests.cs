using DomainEntities = Linkubator.Domain.Entities;
using System.Reflection;
using Xunit.Sdk;
using Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

[Trait("Category", "TestFactory")]
public class EntityTestFactoryTests
{
    [Fact]
    public void SetId_SetsId_WhenEntityIdIsZero()
    {
        Collection collection = DomainEntities.Collection.Create(1, "temp", null);
        long expectedId = 42;

        EntityTestFactory.SetId(collection, expectedId);

        Assert.Equal(expectedId, collection.Id);
    }

    [Fact]
    public void SetId_Throws_WhenEntityIdIsNotZero()
    {
        Collection collection = DomainEntities.Collection.Create(1, "temp", null);
        // Set Id to non-zero via reflection
        PropertyInfo idProperty = typeof(DomainEntities.Bases.BaseEntity).GetProperty(
            nameof(DomainEntities.Bases.BaseEntity.Id),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        idProperty.SetValue(collection, 99L);

        Assert.Throws<EqualException>(() => EntityTestFactory.SetId(collection, 5));
        Assert.Equal(99L, collection.Id);
    }

    [Fact]
    public void AddLink_InvokesInternalAddLinkMethod_AndAddsLink()
    {
        Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 1);
        Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        EntityTestFactory.AddLink(collection, link);

        Assert.Contains(link, collection.Links);
    }

    [Fact]
    public void RemoveLink_InvokesInternalRemoveLinkMethod_AndRemovesLink()
    {
        Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 1);
        Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);
        EntityTestFactory.AddLink(collection, link);

        EntityTestFactory.RemoveLink(collection, link);

        Assert.DoesNotContain(link, collection.Links);
    }

    [Fact]
    public void ResolveIdProperty_ReturnsIdPropertyInfo()
    {
        PropertyInfo propertyInfo = EntityTestFactory.ResolveIdProperty();

        Assert.NotNull(propertyInfo);
        Assert.IsAssignableFrom<System.Reflection.PropertyInfo>(propertyInfo);
        Assert.Equal(nameof(DomainEntities.Bases.BaseEntity.Id), propertyInfo.Name);
        Assert.Equal(typeof(DomainEntities.Bases.BaseEntity), propertyInfo.DeclaringType);
    }

    [Fact]
    public void ResolveInternalMethod_ReturnsMethodInfo_ForAddLink()
    {
        MethodInfo methodInfo = EntityTestFactory.ResolveInternalMethod(typeof(DomainEntities.Collection), "AddLink");

        Assert.NotNull(methodInfo);
        Assert.IsAssignableFrom<System.Reflection.MethodInfo>(methodInfo);
        Assert.Equal("AddLink", methodInfo.Name);
        Assert.Equal(typeof(DomainEntities.Collection), methodInfo.DeclaringType);
    }

    [Fact]
    public void ResolveInternalMethod_ReturnsMethodInfo_ForRemoveLink()
    {
        MethodInfo methodInfo = EntityTestFactory.ResolveInternalMethod(typeof(DomainEntities.Collection), "RemoveLink");

        Assert.NotNull(methodInfo);
        Assert.IsAssignableFrom<System.Reflection.MethodInfo>(methodInfo);
        Assert.Equal("RemoveLink", methodInfo.Name);
        Assert.Equal(typeof(DomainEntities.Collection), methodInfo.DeclaringType);
    }

    [Fact]
    public void ResolveInternalMethod_Throws_WhenMethodNotFound()
    {
        string bogusName = "NonExistentMethod";

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            EntityTestFactory.ResolveInternalMethod(typeof(DomainEntities.Collection), bogusName));
        Assert.Contains("no encontrado", exception.Message);
    }
}