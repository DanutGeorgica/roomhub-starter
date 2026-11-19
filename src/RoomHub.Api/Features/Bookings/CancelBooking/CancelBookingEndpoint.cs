using RoomHub.Api.Domain;
using RoomHub.Api.Services;
using RoomHub.Api.Shared.CommandHandling;
using RoomHub.Api.Shared.Http;

namespace RoomHub.Api.Features.Bookings.CancelBooking;

public sealed record CancelBookingCommand(Guid Id);

public sealed class CancelBookingHandler(BookingService service)
    : ICommandHandler<CancelBookingCommand, Result>
{
    public Result Handle(CancelBookingCommand command)
    {
        return service.Cancel(command.Id);
    }
}

public static class CancelBookingEndpoint
{
    public static void MapCancelBookingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/bookings/{id:guid}/cancel", (Guid id, CancelBookingHandler handler) =>
        {
            var result = handler.Handle(new CancelBookingCommand(id));
            if (!result.IsSuccess)
                return result.Error!.ToProblem();

            return Results.NoContent();
        }).WithTags("Bookings");
    }
}
