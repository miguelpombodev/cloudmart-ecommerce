using Identity.Domain.Entities;
using Mapster;

namespace Identity.Application.Features.Users.Register;

public sealed class RegisterUserMappingConfig : IRegister
{
  public void Register(TypeAdapterConfig config)
  {
    config.NewConfig<User, RegisterUserResponse>()
      .Map(dest => dest.FullName, src => $"{src.Name.FirstName} {src.Name.LastName}")
      .Map(dest => dest.Email, src => src.Email.Address);
  }
}
