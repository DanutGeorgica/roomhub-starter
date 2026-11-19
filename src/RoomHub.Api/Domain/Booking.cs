namespace RoomHub.Api.Domain;

public enum BookingStatus
{
    Active,
    Cancelled
}

public sealed class Booking
{
    private Booking()
    {
        BookedBy = string.Empty;
    }

    private Booking(
        Guid id,
        Guid roomId,
        string bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees)
    {
        Id = id;
        RoomId = roomId;
        BookedBy = bookedBy;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Attendees = attendees;
        Status = BookingStatus.Active;
    }

    public Guid Id { get; private set; }

    public Guid RoomId { get; private set; }

    public string BookedBy { get; private set; }

    public DateTime StartUtc { get; private set; }

    public DateTime EndUtc { get; private set; }

    public int Attendees { get; private set; }

    public BookingStatus Status { get; private set; }

    public static Result<Booking> Create(
        Room room,
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees,
        DateTime nowUtc,
        IEnumerable<Booking> existingBookings)
    {
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

        if (startUtc < nowUtc)
            return Result<Booking>.Failure(
                new Error("StartInPast", "The booking cannot start in the past.", 400));

        var maximumDuration = room.Capacity > 10
            ? TimeSpan.FromHours(2)
            : TimeSpan.FromHours(4);
        if (endUtc - startUtc > maximumDuration)
            return Result<Booking>.Failure(
                new Error(
                    "DurationTooLong",
                    "The booking duration exceeds the room limit.",
                    400));

        var normalizedBookedBy = bookedBy.Trim();
        var bookings = existingBookings.ToList();

        if (bookings.Any(existing =>
                existing.RoomId == room.Id &&
                existing.Status == BookingStatus.Active &&
                existing.StartUtc < endUtc &&
                startUtc < existing.EndUtc))
        {
            return Result<Booking>.Failure(
                new Error("RoomOccupied", "The room is occupied in this interval.", 409));
        }

        var activeBookingsOnDay = bookings.Count(existing =>
            existing.Status == BookingStatus.Active &&
            string.Equals(existing.BookedBy, normalizedBookedBy, StringComparison.Ordinal) &&
            existing.StartUtc.Date == startUtc.Date);
        if (activeBookingsOnDay >= 2)
        {
            return Result<Booking>.Failure(
                new Error(
                    "DailyBookingLimitReached",
                    "The participant already has two active bookings on this day.",
                    409));
        }

        return Result<Booking>.Success(new Booking(
            Guid.NewGuid(),
            room.Id,
            normalizedBookedBy,
            startUtc,
            endUtc,
            attendees));
    }

    public Result Cancel(DateTime nowUtc)
    {
        if (Status == BookingStatus.Cancelled)
            return Result.Failure(
                new Error("AlreadyCancelled", "The booking is already cancelled.", 409));

        if (StartUtc - nowUtc < TimeSpan.FromMinutes(60))
            return Result.Failure(
                new Error(
                    "CancellationTooLate",
                    "A booking can only be cancelled at least 60 minutes before it starts.",
                    409));

        Status = BookingStatus.Cancelled;
        return Result.Success();
    }
}
