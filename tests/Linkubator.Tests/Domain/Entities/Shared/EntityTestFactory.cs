using DomainEntities = Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

internal static class EntityTestFactory
{
    private static readonly System.Reflection.PropertyInfo _idProperty = ResolveIdProperty();

    private static readonly System.Reflection.MethodInfo _addLinkMethod = ResolveInternalMethod("AddLink");

    private static readonly System.Reflection.MethodInfo _removeLinkMethod = ResolveInternalMethod("RemoveLink");

    internal static void SetId(DomainEntities.Bases.BaseEntity entity, long id)
    {
        Assert.Equal(0, entity.Id);
        _idProperty.SetValue(entity, id);
    }

    internal static void AddLink(DomainEntities.Collection collection, DomainEntities.Link link)
    {
        InvokeInternal(_addLinkMethod, collection, link);
    }

    internal static void RemoveLink(DomainEntities.Collection collection, DomainEntities.Link link)
    {
        InvokeInternal(_removeLinkMethod, collection, link);
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

    internal static System.Reflection.MethodInfo ResolveInternalMethod(string name)
    {
        System.Reflection.MethodInfo? method = typeof(DomainEntities.Collection).GetMethod(
            name,
            System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);

        if (method is null)
        {
            throw new InvalidOperationException(
                $"{nameof(DomainEntities.Collection)}.{name} no encontrado. Actualiza EntityTestFactory.");
        }

        return method;
    }

    internal static void InvokeInternal(System.Reflection.MethodInfo method, DomainEntities.Collection collection, params object?[] arguments)
    {
        try
        {
            _ = method.Invoke(collection, arguments);
        }
        catch (System.Reflection.TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }
}