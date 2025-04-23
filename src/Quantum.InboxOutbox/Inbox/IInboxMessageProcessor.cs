namespace Quantum.InboxOutbox.Inbox;

public interface IInboxMessageProcessor
{
    Task Process(InboxMessage inboxMessage);
}