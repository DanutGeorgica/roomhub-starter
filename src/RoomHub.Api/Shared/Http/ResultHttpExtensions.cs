using RoomHub.Api.Domain;

namespace RoomHub.Api.Shared.Http;

public static class ResultHttpExtensions
{
    public static IResult ToProblem(this Error error)
    {
        return Results.Problem(
            detail: error.Message,
            statusCode: error.StatusCode,
            title: error.Code);
    }
}
