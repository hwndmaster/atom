using System.Threading.Tasks;

namespace Genius.Atom.Infrastructure.Commands
{
    public interface ICommandHandler
    {
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler
        where TCommand : ICommandMessage
    {
        Task ProcessAsync(TCommand command);
    }

    public interface ICommandHandler<in TCommand, TResult> : ICommandHandler where TCommand : ICommandMessageExchange<TResult>
    {
        Task<TResult> ProcessAsync(TCommand command);
    }
}
