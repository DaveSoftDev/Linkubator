namespace Linkubator.Application;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Get(string contextName);
}
