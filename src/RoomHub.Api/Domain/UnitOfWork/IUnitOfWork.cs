namespace RoomHub.Api.Domain.UnitOfWork;

public interface IUnitOfWork
{
    void SaveChanges();

    TResult ExecuteInTransaction<TResult>(Func<TResult> operation);
}
