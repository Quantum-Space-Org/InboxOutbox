using Microsoft.Extensions.Logging;
using Quantum.ServiceBus;

namespace Quantum.InboxOutbox.Inbox.Subscriber;

public class InboxMessageSubscriber : IEventBusMessageSubscriber
{
    private readonly InboxOutboxDbContext _context;

    private readonly ILogger<InboxMessageSubscriber> _logger;

    public InboxMessageSubscriber(InboxOutboxDbContext context, ILogger<InboxMessageSubscriber> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Subscribe(ConsumerMessageResult message)
    {
        _logger.LogInformation("Pull message from event bus. message {@message}", message);

        var entity = _context.InboxMessages.Add(CreateInbox(message));
        var task = _context.SaveChangesAsync();
        task.Wait();

        _logger.LogInformation($"the message successfully saved to inbox. inbox message id is {entity.Entity.Id}");
    }

    private static InboxMessage CreateInbox(ConsumerMessageResult message)
        => new(Guid.NewGuid().ToString(), DateTime.UtcNow, message.MessageType, message.Message);
}