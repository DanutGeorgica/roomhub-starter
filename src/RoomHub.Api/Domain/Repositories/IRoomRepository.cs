namespace RoomHub.Api.Domain.Repositories;

public interface IRoomRepository
{
    void Add(Room room);

    Room? Get(Guid id);
}
