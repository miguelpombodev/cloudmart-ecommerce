using MediatR;

namespace BuildingBlocks.Abstractions;

public interface IDomainEvent : INotification
{
	Guid EventId => Guid.NewGuid();
	DateTime OccuredOn => DateTime.Now;
	string? EventType => GetType().AssemblyQualifiedName;
}
