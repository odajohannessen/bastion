using MediatR;

namespace Bastion.SharedKernel;

public abstract class BaseEntity
{
    public List<BaseDomainEvent> Events = new();
}
