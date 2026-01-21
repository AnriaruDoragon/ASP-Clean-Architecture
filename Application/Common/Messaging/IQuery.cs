using Application.Common.Models;
using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Marker interface for queries (operations that read data without side effects).
/// Queries return Result with the requested data.
/// </summary>
/// <typeparam name="TResponse">The type of data returned.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
