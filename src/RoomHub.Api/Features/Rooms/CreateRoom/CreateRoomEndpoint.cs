using RoomHub.Api.Domain;
using RoomHub.Api.Services;
using RoomHub.Api.Shared.CommandHandling;
using RoomHub.Api.Shared.Http;

namespace RoomHub.Api.Features.Rooms.CreateRoom;

public sealed record CreateRoomRequest(string? Name, int Capacity);

public sealed record CreateRoomResponse(Guid Id, string Name, int Capacity);

public sealed record CreateRoomCommand(string? Name, int Capacity);

public sealed class CreateRoomHandler(RoomService service)
    : ICommandHandler<CreateRoomCommand, Result<Room>>
{
    public Result<Room> Handle(CreateRoomCommand command)
    {
        return service.Create(command.Name, command.Capacity);
    }
}

public static class CreateRoomEndpoint
{
    public static void MapCreateRoomEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/rooms", (CreateRoomRequest request, CreateRoomHandler handler) =>
        {
            var result = handler.Handle(new CreateRoomCommand(request.Name, request.Capacity));
            if (!result.IsSuccess)
                return result.Error!.ToProblem();

            var room = result.Value!;
            return Results.Created(
                $"/api/rooms/{room.Id}",
                new CreateRoomResponse(room.Id, room.Name, room.Capacity));
        }).WithTags("Rooms");
    }
}
