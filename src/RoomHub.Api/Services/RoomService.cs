using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Repositories;
using RoomHub.Api.Domain.UnitOfWork;

namespace RoomHub.Api.Services;

public class RoomService(IRoomRepository rooms, IUnitOfWork unitOfWork)
{
    public Result<Room> Create(string? name, int capacity)
    {
        var result = Room.Create(name, capacity);
        if (!result.IsSuccess)
            return result;

        unitOfWork.ExecuteInTransaction(() =>
        {
            rooms.Add(result.Value!);
            return 0;
        });
        return result;
    }

    public Result<Room> Get(Guid id)
    {
        var room = rooms.Get(id);
        if (room is null)
            return Result<Room>.Failure(
                new Error("RoomNotFound", "The room does not exist.", 404));

        return Result<Room>.Success(room);
    }
}
