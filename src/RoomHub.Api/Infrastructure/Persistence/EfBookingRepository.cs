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

    public IReadOnlyList<Booking> ListActiveForRoom(Guid roomId)
    {
        return db.Bookings
            .Where(booking =>
                booking.RoomId == roomId &&
                booking.Status == BookingStatus.Active)
            .ToList();
    }

    public int CountActiveForParticipantOnDate(string bookedBy, DateTime dateUtc)
    {
        var dayStart = dateUtc.Date;
        var nextDayStart = dayStart.AddDays(1);

        return db.Bookings.Count(booking =>
            booking.BookedBy == bookedBy &&
            booking.Status == BookingStatus.Active &&
            booking.StartUtc >= dayStart &&
            booking.StartUtc < nextDayStart);
    }
}
