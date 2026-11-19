using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Repositories;

namespace RoomHub.Api.Infrastructure.Persistence;

public sealed class EfRoomRepository(RoomHubDbContext db) : IRoomRepository
{
    public void Add(Room room)
    {
        db.Rooms.Add(room);
    }

    public Room? Get(Guid id)
    {
        return db.Rooms.Find(id);
    }
}
