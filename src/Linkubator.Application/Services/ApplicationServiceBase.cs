using Linkubator.Application.Constants;
using Linkubator.Application.Logging;
using Microsoft.Extensions.Logging;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Application.Services;

public abstract class ApplicationServiceBase
{
    protected ILogger Logger { get; }
    protected ICurrentUser CurrentUser => _currentUser ?? throw new InvalidOperationException("This service does not have a current user.");
    private readonly ICurrentUser? _currentUser;
    private readonly IUnitOfWorkFactory? _unitOfWorkFactory;

    /// <summary>
    /// Inicializa ApplicationServiceBase con las dependencias necesarias.
    /// </summary>
    protected ApplicationServiceBase(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    /// <summary>
    /// Inicializa ApplicationServiceBase con las dependencias necesarias.
    /// </summary>
    protected ApplicationServiceBase(ILogger logger, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(currentUser);
        Logger = logger;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Inicializa ApplicationServiceBase con las dependencias necesarias.
    /// </summary>
    protected ApplicationServiceBase(ILogger logger, ICurrentUser currentUser, IUnitOfWorkFactory unitOfWorkFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(unitOfWorkFactory);
        Logger = logger;
        _currentUser = currentUser;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    protected object UserContext => _currentUser is null ? (object)new { } : new { UserId = _currentUser.UserId };

    private IUnitOfWorkFactory UnitOfWorkFactory
        => _unitOfWorkFactory ?? throw new InvalidOperationException("This service does not have a unit-of-work factory.");

    /// <summary>
    /// Comprueba que el usuario actual esté autenticado y devuelve el resultado correspondiente.
    /// </summary>
    protected Result EnsureAuthenticated()
    {
        if (CurrentUser.IsAuthenticated)
        {
            return Result.Success();
        }

        SafeLogger.Log(Logger, LogLevel.Warning, null, "Request rejected; user is not authenticated", new { UserId = CurrentUser.UserId });
        return Result.Failure(CommonErrors.NotAuthenticated());
    }

    /// <summary>
    /// Ejecuta la acción dentro de una unidad de trabajo transaccional.
    /// </summary>
    protected async Task<Result<TResponse>> ExecuteInTransaction<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken,
        Func<TRequest, IUnitOfWork, CancellationToken, Task<Result<TResponse>>> operation,
        string failureMessage,
        object? logContext = null)
    {
        try
        {
            await using IUnitOfWork unitOfWork = UnitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            Result<TResponse> result = await operation(request, unitOfWork, cancellationToken);
            if (result.IsSuccess)
            {
                await unitOfWork.Commit(cancellationToken);
            }

            return result;
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, exception, failureMessage, UserContext, logContext);
            return exception.FailDomain<TResponse>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(Logger, LogLevel.Error, exception, failureMessage, UserContext, logContext);
            throw;
        }
    }

    /// <summary>
    /// Ejecuta la acción dentro de una unidad de trabajo transaccional.
    /// </summary>
    protected async Task<Result> ExecuteInTransaction<TRequest>(
        TRequest request,
        CancellationToken cancellationToken,
        Func<TRequest, IUnitOfWork, CancellationToken, Task<Result>> operation,
        string failureMessage,
        object? logContext = null)
    {
        try
        {
            await using IUnitOfWork unitOfWork = UnitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            Result result = await operation(request, unitOfWork, cancellationToken);
            if (result.IsSuccess)
            {
                await unitOfWork.Commit(cancellationToken);
            }

            return result;
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, exception, failureMessage, UserContext, logContext);
            return exception.FailDomain();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(Logger, LogLevel.Error, exception, failureMessage, UserContext, logContext);
            throw;
        }
    }

    /// <summary>
    /// Ejecuta la operación y convierte las excepciones de dominio configuradas en errores de aplicación.
    /// </summary>
    protected async Task<Result<TResponse>> Execute<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken,
        Func<TRequest, CancellationToken, Task<Result<TResponse>>> operation,
        string failureMessage,
        bool handleDomainExceptions = true,
        object? logContext = null)
    {
        try
        {
            return await operation(request, cancellationToken);
        }
        catch (DomainExceptions.DomainException exception) when (handleDomainExceptions)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, exception, failureMessage, UserContext, logContext);
            return exception.FailDomain<TResponse>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(Logger, LogLevel.Error, exception, failureMessage, UserContext, logContext);
            throw;
        }
    }

    /// <summary>
    /// Ejecuta la operación y convierte las excepciones de dominio configuradas en errores de aplicación.
    /// </summary>
    protected async Task<Result<TResponse>> Execute<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken,
        Func<TRequest, IUnitOfWork,CancellationToken, Task<Result<TResponse>>> operation,
        string failureMessage,
        bool handleDomainExceptions = true,
        object? logContext = null)
    {
        try
        {
            await using IUnitOfWork unitOfWork = UnitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            return await operation(request, unitOfWork, cancellationToken);
        }
        catch (DomainExceptions.DomainException exception) when (handleDomainExceptions)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, exception, failureMessage, UserContext, logContext);
            return exception.FailDomain<TResponse>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(Logger, LogLevel.Error, exception, failureMessage, UserContext, logContext);
            throw;
        }
    }

    /// <summary>
    /// Ejecuta la operación y convierte las excepciones de dominio configuradas en errores de aplicación.
    /// </summary>
    protected async Task<Result> Execute<TRequest>(
        TRequest request,
        CancellationToken cancellationToken,
        Func<TRequest, CancellationToken, Task<Result>> operation,
        string failureMessage,
        bool handleDomainExceptions = true,
        object? logContext = null)
    {
        try
        {
            return await operation(request, cancellationToken);
        }
        catch (DomainExceptions.DomainException exception) when (handleDomainExceptions)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, exception, failureMessage, UserContext, logContext);
            return exception.FailDomain();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(Logger, LogLevel.Error, exception, failureMessage, UserContext, logContext);
            throw;
        }
    }

    /// <summary>
    /// Registra el rechazo de la operación y crea un resultado fallido con el error indicado.
    /// </summary>
    protected Result<T> Failure<T>(ApplicationError error, string message, object? request = null)
    {
        SafeLogger.Log(Logger, LogLevel.Warning, null, message, UserContext, request);
        return Result<T>.Failure(error);
    }

    /// <summary>
    /// Registra el rechazo de la operación y crea un resultado fallido con el error indicado.
    /// </summary>
    protected Result Failure(ApplicationError error, string message, object? request = null)
    {
        SafeLogger.Log(Logger, LogLevel.Warning, null, message, UserContext, request);
        return Result.Failure(error);
    }
}
