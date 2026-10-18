using DomainEntities = Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

internal static class EntityTestFactory
{
    private static readonly System.Reflection.PropertyInfo _idProperty = ResolveIdProperty();

    private static readonly System.Reflection.MethodInfo _addLinkMethod = ResolveInternalMethod(typeof(DomainEntities.Collection), "AddLink");

    private static readonly System.Reflection.MethodInfo _removeLinkMethod = ResolveInternalMethod(typeof(DomainEntities.Collection), "RemoveLink");

    private static readonly System.Reflection.MethodInfo _rehydrateLinksMethod = ResolveInternalMethod(typeof(DomainEntities.Collection), "RehydrateLinks");

    private static readonly System.Reflection.MethodInfo _rehydrateCollectionMethod = ResolveInternalMethod(typeof(DomainEntities.Link), "RehydrateCollection");

    internal static void SetId(DomainEntities.Bases.BaseEntity entity, long id)
    {
        Assert.Equal(0, entity.Id);
        _idProperty.SetValue(entity, id);
    }

    internal static void AddLink(DomainEntities.Collection collection, DomainEntities.Link link)
    {
        InvokeInternal(collection, _addLinkMethod, link);
    }

    internal static void RemoveLink(DomainEntities.Collection collection, DomainEntities.Link link)
    {
        InvokeInternal(collection, _removeLinkMethod, link);
    }

    internal static void RehydrateLinks(DomainEntities.Collection collection, params DomainEntities.Link[] links)
    {
        InvokeInternal(collection, _rehydrateLinksMethod, (object)links);
    }

    internal static void RehydrateCollection(DomainEntities.Link link, DomainEntities.Collection collection)
    {
        InvokeInternal(link, _rehydrateCollectionMethod, collection);
    }

    internal static System.Reflection.PropertyInfo ResolveIdProperty()
    {
        System.Reflection.PropertyInfo? property = typeof(DomainEntities.Bases.BaseEntity).GetProperty(
            nameof(DomainEntities.Bases.BaseEntity.Id),
            System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic);

        if (property is null)
        {
            throw new InvalidOperationException(
                $"{nameof(DomainEntities.Bases.BaseEntity)}.{nameof(DomainEntities.Bases.BaseEntity.Id)} no encontrada. Actualiza EntityTestFactory.");
        }

        return property;
    }

    internal static System.Reflection.MethodInfo ResolveInternalMethod(Type type, string name)
    {
        System.Reflection.MethodInfo? method = type.GetMethod(
            name,
            System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic);

        if (method is null)
        {
            throw new InvalidOperationException(
                $"{type.Name}.{name} no encontrado. Actualiza EntityTestFactory.");
        }

        return method;
    }

    internal static void InvokeInternal(object target, System.Reflection.MethodInfo method, params object?[] arguments)
    {
        try
        {
            _ = method.Invoke(target, arguments);
        }
        catch (System.Reflection.TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }
}