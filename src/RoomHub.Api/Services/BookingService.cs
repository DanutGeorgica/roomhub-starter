using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Repositories;
using RoomHub.Api.Domain.UnitOfWork;

namespace RoomHub.Api.Services;

public class BookingService(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public Result<Booking> Create(
        Guid roomId,
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees)
    {
        return unitOfWork.ExecuteInTransaction(() =>
        {
            var room = rooms.Get(roomId);
            if (room is null)
                return Result<Booking>.Failure(
                    new Error("RoomNotFound", "The room does not exist.", 404));

            var result = Booking.Create(
                room,
                bookedBy,
                startUtc,
                endUtc,
                attendees,
                timeProvider.GetUtcNow().UtcDateTime,
                bookings.ListAll());
            if (!result.IsSuccess)
                return result;

            bookings.Add(result.Value!);
            return result;
        });
    }

    public Result<Booking> Get(Guid id)
    {
        var booking = bookings.Get(id);
        if (booking is null)
            return Result<Booking>.Failure(
                new Error("BookingNotFound", "The booking does not exist.", 404));

        return Result<Booking>.Success(booking);
    }

    public Result Cancel(Guid id)
    {
        return unitOfWork.ExecuteInTransaction(() =>
        {
            var booking = bookings.Get(id);
            if (booking is null)
                return Result.Failure(
                    new Error("BookingNotFound", "The booking does not exist.", 404));

            return booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);
        });
    }
}
