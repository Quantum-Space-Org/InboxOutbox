using Quantum.Domain;

namespace Quantum.InboxOutbox.Outbox.OutsideEvent;

public interface IOutsideEventFactoryRegistry
{
    Core.Nullable<IOutsideEventFactory<T>> Resolve<T>() where T : DomainEvent;
    Core.Nullable<object> Resolve(Type type);
}