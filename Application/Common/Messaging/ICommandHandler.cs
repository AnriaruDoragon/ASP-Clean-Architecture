using Application.Common.Models;
using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Handler for commands that don't return a value.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Handler for commands that return a value on success.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
