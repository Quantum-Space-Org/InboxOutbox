using Quantum.Command.Pipeline;
using Quantum.InboxOutbox.Outbox.OutsideEvent;

namespace Quantum.InboxOutbox.Outbox.PipelineStage;

public class StoreDomainEventsToOutboxStage : IAmAPipelineStage
{
    private readonly InboxOutboxDbContext _inboxOutboxDbContext;
    private readonly IOutsideEventFactoryRegistry _factoryRegistry;

    public StoreDomainEventsToOutboxStage(InboxOutboxDbContext inboxOutboxDbContext, IOutsideEventFactoryRegistry factoryRegistry)
    {
        _inboxOutboxDbContext = inboxOutboxDbContext;
        _factoryRegistry = factoryRegistry;
    }

    public override async Task Process<TContext>(TContext command, StageContext context)
    {
        var domainEvents = context.GetDomainEvents();

        foreach (var (_, queuedDomainEvent) in domainEvents)
        {
            await Store<TContext>(queuedDomainEvent);
        }

        await _inboxOutboxDbContext.SaveChangesAsync();
    }

    private async Task Store<TContext>(QueuedEvents queuedDomainEvent)
    {
        foreach (var isADomainEvent in queuedDomainEvent.Events)
        {
            await Store(isADomainEvent);
        }
    }

    private async Task Store(IsADomainEvent isADomainEvent)
    {
        var factory = ResolveFactory();

        if (factory.IsPresent() is false) return;

        var outboxMessage = ToOutboxMessage(CreateOutsideEvent());
        await _inboxOutboxDbContext.OutboxMessages.AddAsync(outboxMessage);


        IAmOutsideEvent? CreateOutsideEvent()
        {
            var createMethod = factory.GetValue().GetType().GetMethod("Create");

            var outsideEvent = (IAmOutsideEvent)
                createMethod.Invoke(factory.GetValue(), new object[] { isADomainEvent });
            return outsideEvent;
        }

        Core.Nullable<object> ResolveFactory()
        {
            return _factoryRegistry.Resolve(Type.GetType(isADomainEvent.MessageMetadata.Type));
        }
    }

    private static OutboxMessage ToOutboxMessage(IAmOutsideEvent domainEvent)
        => new(domainEvent.Metadata.Id,
            DateTime.Now,
            domainEvent.Metadata.Type,
            Newtonsoft.Json.JsonConvert.SerializeObject(domainEvent),
            true);
}