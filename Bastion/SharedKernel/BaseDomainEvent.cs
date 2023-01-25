using MediatR;

namespace Bastion.SharedKernel;

public class BaseDomainEvent : INotification
{
    public DateTimeOffset DateOccurred { get; protected set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
}
