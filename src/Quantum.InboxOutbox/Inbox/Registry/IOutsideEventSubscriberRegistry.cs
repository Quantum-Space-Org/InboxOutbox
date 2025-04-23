namespace Quantum.InboxOutbox.Inbox.Registry;

public interface IOutsideEventSubscriberRegistry
{
    Core.Nullable<object> Resolve(Type type);
}