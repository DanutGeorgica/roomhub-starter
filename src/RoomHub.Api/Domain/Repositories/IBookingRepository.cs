namespace RoomHub.Api.Domain.Repositories;

public interface IBookingRepository
{
    void Add(Booking booking);

    Booking? Get(Guid id);

    IReadOnlyList<Booking> ListAll();
}
