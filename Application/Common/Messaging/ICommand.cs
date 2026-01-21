using Application.Common.Models;
using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Marker interface for commands (operations that change state).
/// Commands return Result to indicate success/failure.
/// </summary>
public interface ICommand : IRequest<Result>;

/// <summary>
/// Marker interface for commands that return a value on success.
/// </summary>
/// <typeparam name="TResponse">The type of value returned on success.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
