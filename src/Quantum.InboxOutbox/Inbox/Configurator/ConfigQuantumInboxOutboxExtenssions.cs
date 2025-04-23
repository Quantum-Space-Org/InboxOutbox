using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.InboxOutbox.Inbox.Registry;
using Quantum.InboxOutbox.Inbox.Subscriber;
using Quantum.ServiceBus;

namespace Quantum.InboxOutbox.Inbox.Configurator;

public static class ConfigQuantumInboxExtensions
{
    public static ConfigQuantumInboxOutboxBuilder ConfigQuantumInbox(this QuantumServiceCollection collection)
    {
        return new ConfigQuantumInboxOutboxBuilder(collection);
    }

    public static ConfigQuartzJobsBuilder ConfigInboxJobs(this QuantumServiceCollection collection)
    {
        return new ConfigQuartzJobsBuilder(collection);
    }
}
public class ConfigQuantumInboxOutboxBuilder
{
    private readonly QuantumServiceCollection _collection;

    public ConfigQuantumInboxOutboxBuilder(QuantumServiceCollection collection)
        => _collection = collection;
    public QuantumServiceCollection ConfigureDefaults(string topicName, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Any() is false)
            throw new Exception($"assemblies is null of empty");

        return RegisterInboxMessageSubscriber()
            .RegisterSubscribers(assemblies)
            .RegisterDefaultEventBusMessageSubscriber()
            .RegisterOutsideEventSubscriberRegistry()
            .RegisterDefaultInboxMessageProcessor()
            .StartServiceBusConsuming(topicName)
            .and();
    }

    private ConfigQuantumInboxOutboxBuilder StartServiceBusConsuming(string topicName)
    {
        using var scope = _collection.Collection.BuildServiceProvider().CreateScope();
        var serviceBus = scope.ServiceProvider.GetRequiredService<IServiceBus>();
        var eventBusMessageSubscriber = scope.ServiceProvider
            .GetRequiredService<IEventBusMessageSubscriber>();

        var topicSpecification = new TopicSpecification
        {
            Name = topicName,
        };

        var task = serviceBus.CreateTopicIfNotExists(topicSpecification);
        task.Wait();
        serviceBus.StartConsuming(topicSpecification.Name, eventBusMessageSubscriber);

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterInboxMessageSubscriber(ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                _collection.Collection.AddSingleton<InboxMessageSubscriber>();
                break;
            case ServiceLifetime.Scoped:
                _collection.Collection.AddScoped<InboxMessageSubscriber>();
                break;
            case ServiceLifetime.Transient:
                _collection.Collection.AddTransient<InboxMessageSubscriber>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterInboxMessageProcessor(ServiceLifetime serviceLifetime)
    {
        RegisterInboxMessageProcessor<InboxMessageProcessor>(serviceLifetime);
        return this;
    }

    private ConfigQuantumInboxOutboxBuilder RegisterDefaultInboxMessageProcessor()
    {
        _collection.Collection.AddSingleton<IInboxMessageProcessor>(x =>
        {
            var outsideEventSubscriberRegistry = x.GetRequiredService<IOutsideEventSubscriberRegistry>();
            var inboxOutboxDbContext = x.GetRequiredService<InboxOutboxDbContext>();
            return new InboxMessageProcessor(outsideEventSubscriberRegistry, inboxOutboxDbContext);
        });

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterInboxMessageProcessor<T>(ServiceLifetime serviceLifetime)
        where T : class, IInboxMessageProcessor
    {
        _collection.Collection.Add(new ServiceDescriptor(typeof(IInboxMessageProcessor), typeof(T), serviceLifetime));
        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutsideEventSubscriberRegistry()
    {
        _collection.Collection.AddSingleton<IOutsideEventSubscriberRegistry, OutsideEventSubscriberRegistry>();
        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterSubscribers(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
            RegisterSubscribers(assembly);

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterDefaultEventBusMessageSubscriber()
    {
        _collection.Collection.AddTransient<IEventBusMessageSubscriber, InboxMessageSubscriber>();
        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterEventBusMessageSubscriber<T>() where T : class, IEventBusMessageSubscriber
    {
        _collection.Collection.AddTransient<IEventBusMessageSubscriber, T>();
        return this;
    }

    public QuantumServiceCollection and()
    {
        return _collection;
    }


    private void RegisterSubscribers(Assembly assembly)
    {
        var subscribers =
            assembly.GetTypes()
                .Where(t =>
                    t.BaseType != null &&
                    t.BaseType.Name == typeof(IWantToSubscribeToOutsideEvent<>).Name);

        foreach (var subscriber in subscribers)
            _collection.Collection.AddTransient(subscriber.BaseType, subscriber);
    }
}