namespace BuildingBlocks.Abstractions;

public interface ICurrentUser
{
  Guid UserId { get; }

  bool IsAuthenticated { get; }

  string Email { get; }

  List<string> Roles { get; }
}
