namespace Quantum.InboxOutbox.Inbox;

using Quantum.InboxOutbox;

public class NullInboxMessageProcessor : IInboxMessageProcessor
{
    public Task Process(InboxMessage inboxMessage)
    {
        return Task.CompletedTask;
    }
}