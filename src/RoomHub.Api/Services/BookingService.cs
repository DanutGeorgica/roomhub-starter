using RoomHub.Api.Domain;
using RoomHub.Api.Models;
using RoomHub.Api.Repositories;

namespace RoomHub.Api.Services;

public class BookingService(RoomRepository rooms, BookingRepository bookings)
{
    public Result<Booking> Create(
        Guid roomId,
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees)
    {
        var room = rooms.Get(roomId);
        if (room is null)
            return Result<Booking>.Failure(
                new Error("RoomNotFound", "The room does not exist.", 404));

        if (string.IsNullOrWhiteSpace(bookedBy))
            return Result<Booking>.Failure(
                new Error("InvalidBookedBy", "BookedBy is required.", 400));

        if (endUtc <= startUtc)
            return Result<Booking>.Failure(
                new Error("InvalidRange", "The end must be after the start.", 400));

        if (attendees <= 0)
            return Result<Booking>.Failure(
                new Error("InvalidAttendees", "Attendees must be positive.", 400));

        if (attendees > room.Capacity)
            return Result<Booking>.Failure(
                new Error(
                    "AttendeesExceedCapacity",
                    "The room does not have enough seats.",
                    400));

        foreach (var existing in bookings.ListForRoom(roomId))
        {
            if (existing.StartUtc <= endUtc && startUtc <= existing.EndUtc)
                return Result<Booking>.Failure(
                    new Error("RoomOccupied", "The room is occupied in this interval.", 409));
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            BookedBy = bookedBy.Trim(),
            StartUtc = startUtc,
            EndUtc = endUtc,
            Attendees = attendees,
            Status = BookingStatus.Active
        };

        bookings.Add(booking);
        return Result<Booking>.Success(booking);
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
        var result = Get(id);
        if (!result.IsSuccess)
            return Result.Failure(result.Error!);

        result.Value!.Status = BookingStatus.Cancelled;
        return Result.Success();
    }
}
