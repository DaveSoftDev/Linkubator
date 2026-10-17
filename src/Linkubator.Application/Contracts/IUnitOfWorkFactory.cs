namespace Linkubator.Application.Contracts;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Get(string contextName);
}
