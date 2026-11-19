using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain.UnitOfWork;

namespace RoomHub.Api.Infrastructure.Persistence;

public sealed class EfUnitOfWork(RoomHubDbContext db) : IUnitOfWork
{
    public void SaveChanges()
    {
        db.SaveChanges();
    }

    public TResult ExecuteInTransaction<TResult>(Func<TResult> operation)
    {
        using var transaction = db.Database.BeginTransaction();
        var result = operation();
        db.SaveChanges();
        transaction.Commit();
        return result;
    }
}
