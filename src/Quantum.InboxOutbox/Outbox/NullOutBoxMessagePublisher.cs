namespace Quantum.InboxOutbox.Outbox;

using Quantum.InboxOutbox;

public class NullOutBoxMessagePublisher : IOutBoxMessagePublisher
{
    public Task Publish(OutboxMessage outboxMessages)
    {
        return Task.CompletedTask;
    }
}