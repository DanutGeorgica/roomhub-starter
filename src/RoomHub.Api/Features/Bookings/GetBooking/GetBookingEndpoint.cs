using RoomHub.Api.Domain;
using RoomHub.Api.Services;
using RoomHub.Api.Shared.CommandHandling;
using RoomHub.Api.Shared.Http;

namespace RoomHub.Api.Features.Bookings.GetBooking;

public sealed record GetBookingResponse(
    Guid Id,
    Guid RoomId,
    string BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees,
    string Status);

public sealed record GetBookingQuery(Guid Id);

public sealed class GetBookingHandler(BookingService service)
    : ICommandHandler<GetBookingQuery, Result<Booking>>
{
    public Result<Booking> Handle(GetBookingQuery query)
    {
        return service.Get(query.Id);
    }
}

public static class GetBookingEndpoint
{
    public static void MapGetBookingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bookings/{id:guid}", (Guid id, GetBookingHandler handler) =>
        {
            var result = handler.Handle(new GetBookingQuery(id));
            if (!result.IsSuccess)
                return result.Error!.ToProblem();

            var booking = result.Value!;
            return Results.Ok(new GetBookingResponse(
                booking.Id,
                booking.RoomId,
                booking.BookedBy,
                booking.StartUtc,
                booking.EndUtc,
                booking.Attendees,
                booking.Status.ToString()));
        }).WithTags("Bookings");
    }
}
