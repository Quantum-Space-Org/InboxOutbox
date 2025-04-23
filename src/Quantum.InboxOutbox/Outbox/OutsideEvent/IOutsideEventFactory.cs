namespace Quantum.InboxOutbox.Outbox.OutsideEvent;

public interface IOutsideEventFactory<in T> where T : DomainEvent
{
    IAmOutsideEvent Create(T @event);
}