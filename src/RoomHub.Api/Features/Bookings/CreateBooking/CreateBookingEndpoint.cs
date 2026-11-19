using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Repositories;
using RoomHub.Api.Domain.UnitOfWork;
using RoomHub.Api.Shared.CommandHandling;
using RoomHub.Api.Shared.Http;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public sealed record CreateBookingRequest(
    string? BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees);

public sealed record CreateBookingResponse(Guid Id, Guid RoomId, string Status);

public sealed record CreateBookingCommand(
    Guid RoomId,
    string? BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees);

public sealed class CreateBookingHandler(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateBookingCommand, Result<Booking>>
{
    public Result<Booking> Handle(CreateBookingCommand command)
    {
        return unitOfWork.ExecuteInTransaction(() =>
        {
            var room = rooms.Get(command.RoomId);
            if (room is null)
                return Result<Booking>.Failure(
                    new Error("RoomNotFound", "The room does not exist.", 404));

            var normalizedBookedBy = command.BookedBy?.Trim();
            var activeBookingsOnDay = string.IsNullOrWhiteSpace(normalizedBookedBy)
                ? 0
                : bookings.CountActiveForParticipantOnDate(
                    normalizedBookedBy,
                    command.StartUtc);
            var result = Booking.Create(
                room,
                command.BookedBy,
                command.StartUtc,
                command.EndUtc,
                command.Attendees,
                timeProvider.GetUtcNow().UtcDateTime,
                bookings.ListActiveForRoom(command.RoomId),
                activeBookingsOnDay);
            if (!result.IsSuccess)
                return result;

            bookings.Add(result.Value!);
            return result;
        });
    }
}

public static class CreateBookingEndpoint
{
    public static void MapCreateBookingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/rooms/{roomId:guid}/bookings",
            (Guid roomId, CreateBookingRequest request, CreateBookingHandler handler) =>
            {
                var result = handler.Handle(new CreateBookingCommand(
                    roomId,
                    request.BookedBy,
                    request.StartUtc,
                    request.EndUtc,
                    request.Attendees));
                if (!result.IsSuccess)
                    return result.Error!.ToProblem();

                var booking = result.Value!;
                return Results.Created(
                    $"/api/bookings/{booking.Id}",
                    new CreateBookingResponse(
                        booking.Id,
                        booking.RoomId,
                        booking.Status.ToString()));
            }).WithTags("Bookings");
    }
}
