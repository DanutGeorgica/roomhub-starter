using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Repositories;

namespace RoomHub.Api.Infrastructure.Persistence;

public sealed class EfBookingRepository(RoomHubDbContext db) : IBookingRepository
{
    public void Add(Booking booking)
    {
        db.Bookings.Add(booking);
    }

    public Booking? Get(Guid id)
    {
        return db.Bookings.Find(id);
    }

    public IReadOnlyList<Booking> ListAll()
    {
        return db.Bookings.ToList();
    }
}
