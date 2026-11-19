namespace RoomHub.Api.Domain;

public sealed class Room
{
    private Room(Guid id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public Guid Id { get; }

    public string Name { get; }

    public int Capacity { get; }

    public static Result<Room> Create(string? name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Room>.Failure(
                new Error("InvalidName", "Room name is required.", 400));

        if (capacity <= 0)
            return Result<Room>.Failure(
                new Error("InvalidCapacity", "Capacity must be positive.", 400));

        return Result<Room>.Success(new Room(Guid.NewGuid(), name.Trim(), capacity));
    }
}
