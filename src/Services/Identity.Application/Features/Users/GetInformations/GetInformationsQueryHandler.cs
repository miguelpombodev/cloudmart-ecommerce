using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;

namespace Identity.Application.Features.Users.GetInformations;

public sealed class GetInformationsQueryHandler: IQueryHandler<GetInformationsQuery, Result<GetInformationsResponse>>
{

  private readonly IUserRepository _repository;

  public GetInformationsQueryHandler(IUserRepository repository)
  {
    _repository = repository;
  }


  public async Task<Result<GetInformationsResponse>> Handle(GetInformationsQuery request, CancellationToken cancellationToken)
  {
    User? checkUser = await _repository.FindByIdAsync(request.userId, cancellationToken);

    if (checkUser is null)
    {
      return Result<GetInformationsResponse>.Failure(Error.Conflict("User not registered!"));
    }

    var user = new GetInformationsResponse(
      checkUser.Name.ToString(),
      checkUser.Email.Address,
      checkUser.UserAvatar is null,
      checkUser.UserAvatar?.AvatarUrl,
      checkUser.Name.Initials,
      checkUser.CreatedAt);

    return Result<GetInformationsResponse>.Success(user);
  }
}
