using Quantum.Core;
using Quantum.ServiceBus;

namespace Quantum.InboxOutbox.Outbox;

public class OutBoxMessagePublisher : IOutBoxMessagePublisher
{
    private readonly InboxOutboxDbContext _dbContext;
    private readonly string _topicName;
    private readonly IServiceBus _serviceBus;

    public OutBoxMessagePublisher(string topicName, IServiceBus serviceBus, InboxOutboxDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(topicName))
            throw new ArgumentNullException(nameof(topicName));

        _topicName = topicName;
        _serviceBus = serviceBus;
        _dbContext = dbContext;
    }

    public async Task Publish(OutboxMessage outboxMessage)
    {
        try
        {
            await _serviceBus.Publish(_topicName, CreateMessage(outboxMessage));

            outboxMessage.SetSeenAt(DateTime.UtcNow);
            outboxMessage.SetSeen();

            _dbContext.OutboxMessages.Update(outboxMessage);
        }
        catch (Exception e)
        {
            // TODO handle exception(s) !
            Console.WriteLine(e);
        }

        await _dbContext.SaveChangesAsync();
    }

    private static Message<string, string> CreateMessage(OutboxMessage outboxMessage)
    {
        return new Message<string, string>(outboxMessage.Id, outboxMessage.Type, outboxMessage.Event.Serialize());
    }
}