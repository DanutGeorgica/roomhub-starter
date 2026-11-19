namespace RoomHub.Api.Domain;

public sealed record Error(string Code, string Message, int StatusCode);
