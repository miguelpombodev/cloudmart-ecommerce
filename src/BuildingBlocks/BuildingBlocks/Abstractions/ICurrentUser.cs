namespace BuildingBlocks.Abstractions;

public interface ICurrentUser
{
  Guid UserId { get; }

  string Email { get; }

  List<string> Roles { get; }
}
