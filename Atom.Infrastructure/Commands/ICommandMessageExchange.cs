namespace Genius.Atom.Infrastructure.Commands;

#pragma warning disable S2326 // Unused type parameters should be removed

public interface ICommandMessageExchange<in TResult> : ICommandMessage
{
}
