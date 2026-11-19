using RoomHub.Api.Domain;
using RoomHub.Api.Models;
using RoomHub.Api.Services;
using RoomHub.Api.Shared.CommandHandling;
using RoomHub.Api.Shared.Http;

namespace RoomHub.Api.Features.Rooms.GetRoom;

public sealed record GetRoomResponse(Guid Id, string Name, int Capacity);

public sealed record GetRoomQuery(Guid Id);

public sealed class GetRoomHandler(RoomService service)
    : ICommandHandler<GetRoomQuery, Result<Room>>
{
    public Result<Room> Handle(GetRoomQuery query)
    {
        return service.Get(query.Id);
    }
}

public static class GetRoomEndpoint
{
    public static void MapGetRoomEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rooms/{id:guid}", (Guid id, GetRoomHandler handler) =>
        {
            var result = handler.Handle(new GetRoomQuery(id));
            if (!result.IsSuccess)
                return result.Error!.ToProblem();

            var room = result.Value!;
            return Results.Ok(new GetRoomResponse(room.Id, room.Name, room.Capacity));
        }).WithTags("Rooms");
    }
}
