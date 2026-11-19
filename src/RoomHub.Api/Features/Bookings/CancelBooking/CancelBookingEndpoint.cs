using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Repositories;
using RoomHub.Api.Domain.UnitOfWork;
using RoomHub.Api.Shared.CommandHandling;
using RoomHub.Api.Shared.Http;

namespace RoomHub.Api.Features.Bookings.CancelBooking;

public sealed record CancelBookingCommand(Guid Id);

public sealed class CancelBookingHandler(
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CancelBookingCommand, Result>
{
    public Result Handle(CancelBookingCommand command)
    {
        return unitOfWork.ExecuteInTransaction(() =>
        {
            var booking = bookings.Get(command.Id);
            if (booking is null)
                return Result.Failure(
                    new Error("BookingNotFound", "The booking does not exist.", 404));

            return booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);
        });
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
