using RoomHub.Api.Domain;
using RoomHub.Api.Models;
using RoomHub.Api.Services;
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

public sealed class CreateBookingHandler(BookingService service)
    : ICommandHandler<CreateBookingCommand, Result<Booking>>
{
    public Result<Booking> Handle(CreateBookingCommand command)
    {
        return service.Create(
            command.RoomId,
            command.BookedBy,
            command.StartUtc,
            command.EndUtc,
            command.Attendees);
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
