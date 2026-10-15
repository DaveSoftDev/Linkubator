using System.Reflection;

namespace Linkubator.Tests.Domain.Policies;

/// <summary>
/// Invoca métodos privados estáticos de las políticas para poder probar los guardas
/// defensivos que la API pública no puede alcanzar, siguiendo el mismo criterio que
/// <c>EntityTestFactory</c> usa para los miembros internos de las entidades. Las
/// excepciones se desenvuelven para poder afirmar sobre la excepción de dominio.
/// </summary>
internal static class PrivatePolicyInvoker
{
    internal static object? InvokeStatic(Type policyType, string methodName, params object?[] arguments)
    {
        MethodInfo? method = policyType.GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);

        if (method is null)
        {
            throw new InvalidOperationException(
                $"{policyType.Name}.{methodName} no encontrado. Actualiza PrivatePolicyInvoker.");
        }

        try
        {
            return method.Invoke(null, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }
}
