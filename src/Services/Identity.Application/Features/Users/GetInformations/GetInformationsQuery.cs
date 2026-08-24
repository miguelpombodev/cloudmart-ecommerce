using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Identity.Application.Features.Users.GetInformations;

public sealed record GetInformationsQuery(Guid userId) : IQuery<Result<GetInformationsResponse>>;
