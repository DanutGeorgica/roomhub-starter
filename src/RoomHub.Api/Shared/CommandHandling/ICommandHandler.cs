namespace RoomHub.Api.Shared.CommandHandling;

public interface ICommandHandler<in TCommand, out TResult>
{
    TResult Handle(TCommand command);
}
