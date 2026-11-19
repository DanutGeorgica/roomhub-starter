using RoomHub.Api.Domain;
using RoomHub.Api.Models;
using RoomHub.Api.Repositories;

namespace RoomHub.Api.Services;

public class RoomService(RoomRepository rooms)
{
    public Result<Room> Create(string? name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Room>.Failure(
                new Error("InvalidName", "Room name is required.", 400));

        if (capacity <= 0)
            return Result<Room>.Failure(
                new Error("InvalidCapacity", "Capacity must be positive.", 400));

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Capacity = capacity
        };

        rooms.Add(room);
        return Result<Room>.Success(room);
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
