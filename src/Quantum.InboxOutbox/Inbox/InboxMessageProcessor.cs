using Quantum.Core;
using Quantum.InboxOutbox.Inbox.Registry;

namespace Quantum.InboxOutbox.Inbox;

public class InboxMessageProcessor : IInboxMessageProcessor
{
    private readonly InboxOutboxDbContext _context;
    private readonly IOutsideEventSubscriberRegistry _registry;

    public InboxMessageProcessor(IOutsideEventSubscriberRegistry registry, InboxOutboxDbContext context)
    {
        _registry = registry;
        _context = context;
    }

    public async Task Process(InboxMessage inboxMessage)
    {
        var type = Type.GetType(inboxMessage.Type);

        if (type != null)
        {
            var subscriber = _registry.Resolve(type);

            if (subscriber.IsPresent())
                await CallSubscriber(inboxMessage, subscriber, type);

            await UpdateEntityAsSeen(inboxMessage);
        }
        else
        {
            await UpdateEntityAsSeen(inboxMessage);
        }
    }

    private async Task CallSubscriber(InboxMessage inboxMessage, Core.Nullable<object> subscriber, Type type)
    {
        var methodInfo = subscriber.GetValue().GetType().GetMethod("Subscribe");
        var deserialize = inboxMessage.Event.Deserialize(type);

        var amOutsideEvent = deserialize as IAmOutsideEvent;

        var result = (Task)methodInfo.Invoke(subscriber.GetValue(), new[] { amOutsideEvent, });

        await result;
    }

    private async Task UpdateEntityAsSeen(InboxMessage inboxMessage)
    {
        inboxMessage.SetSeenAt(DateTime.UtcNow);
        inboxMessage.SetSeen();

        _context.InboxMessages.Update(inboxMessage);

        await _context.SaveChangesAsync();
    }
}