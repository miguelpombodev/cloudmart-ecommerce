using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using Polly;
using Polly.Registry;

namespace Identity.Application.Features.Users.GetInformations;

public sealed class GetInformationsQueryHandler : IQueryHandler<GetInformationsQuery, Result<GetInformationsResponse>>
{
  private readonly IUserRepository _repository;

  private readonly ResiliencePipeline _databasePipeline;

  public GetInformationsQueryHandler(
    IUserRepository repository,
    ResiliencePipelineProvider<string> pipelineProvider
  )
  {
    _repository = repository;
    _databasePipeline = pipelineProvider.GetPipeline("database-operations");
  }


  public async Task<Result<GetInformationsResponse>> Handle(
    GetInformationsQuery request,
    CancellationToken cancellationToken)
  {
    User? checkUser = await _databasePipeline.ExecuteAsync(async ct => await _repository.FindByIdAsync(
        request.userId, ct
      )
    );

    if (checkUser is null)
    {
      return Result<GetInformationsResponse>.Failure(Error.Conflict("User not registered!"));
    }

    GetInformationsResponse user = CreateResponse(checkUser);

    return Result<GetInformationsResponse>.Success(user);
  }

  private static GetInformationsResponse CreateResponse(User user)
  {
    var response = new GetInformationsResponse(
      user.Name.ToString(),
      user.Email.Address,
      user.UserAvatar is not null,
      user.UserAvatar?.AvatarUrl,
      user.Name.Initials,
      user.CreatedAt);

    return response;
  }
}
