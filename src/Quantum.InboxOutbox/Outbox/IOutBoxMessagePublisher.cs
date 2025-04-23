namespace Quantum.InboxOutbox.Outbox;

public interface IOutBoxMessagePublisher
{
    Task Publish(OutboxMessage outboxMessages);
}