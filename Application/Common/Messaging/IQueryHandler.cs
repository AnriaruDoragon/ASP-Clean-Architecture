using Application.Common.Models;
using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Handler for queries.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
