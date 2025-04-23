
namespace Quantum.InboxOutbox.Inbox.Subscriber;

public interface IWantToSubscribeToOutsideEvent<in T> where T : IAmOutsideEvent
{
    Task Subscribe(T @event);
}