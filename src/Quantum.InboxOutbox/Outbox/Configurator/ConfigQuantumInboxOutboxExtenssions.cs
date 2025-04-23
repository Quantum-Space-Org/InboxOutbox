using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.InboxOutbox.Outbox.OutsideEvent;
using Quantum.ServiceBus;

namespace Quantum.InboxOutbox.Outbox.Configurator;

public static class ConfigQuantumOutboxExtensions
{
    public static ConfigQuantumInboxOutboxBuilder ConfigQuantumOutbox(this QuantumServiceCollection collection) =>
        new(collection);

    public static ConfigQuartzJobsBuilder ConfigOutboxJobs(this QuantumServiceCollection collection)
    {
        return new ConfigQuartzJobsBuilder(collection);
    }
}

public class ConfigQuantumInboxOutboxBuilder(QuantumServiceCollection collection)
{
    public QuantumServiceCollection ConfigureDefaults(string topicName, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Any() is false)
            throw new Exception($"assemblies is null of empty");

        return RegisterOutsideEventFactoryRegistry(assemblies)
        .RegisterOutBoxMessagePublisher(topicName).and();
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutBoxMessagePublisher<T>()
        where T : class, IOutBoxMessagePublisher
    {
        collection.Collection.Add(new ServiceDescriptor(typeof(IOutBoxMessagePublisher), typeof(T), ServiceLifetime.Transient));
        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutBoxMessagePublisher()
    {
        collection.Collection.Add(new ServiceDescriptor(typeof(IOutBoxMessagePublisher),
            typeof(OutBoxMessagePublisher),
            ServiceLifetime.Transient));

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutBoxMessagePublisher(string topicName)
    {
        collection.Collection.AddTransient<IOutBoxMessagePublisher>(serviceProvider =>
        {
            var serviceBus = serviceProvider.GetRequiredService<IServiceBus>();

            var topicSpecification = new TopicSpecification
            {
                Name = topicName,
            };

            var task = serviceBus.CreateTopicIfNotExists(topicSpecification);
            task.Wait();

            var inboxOutboxDbContext = serviceProvider.GetRequiredService<InboxOutboxDbContext>();

            return new OutBoxMessagePublisher(
                topicSpecification.Name, serviceBus, inboxOutboxDbContext);
        });

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutBoxMessagePublisher(Func<IServiceProvider, IOutBoxMessagePublisher> factory)
    {
        collection.Collection.AddTransient(factory);

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutsideEventSubscriberRegistry<T>(T registry)
        where T : class, IOutsideEventFactoryRegistry
    {
        collection.Collection.AddSingleton<IOutsideEventFactoryRegistry>(registry);

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutsideEventFactoryRegistry(params Assembly[] assemblies)
    {
        var outsideEventFactoryRegistry = OutsideEventFactoryRegistry.Builder.Register(assemblies).Build();

        collection.Collection.AddSingleton<IOutsideEventFactoryRegistry>(outsideEventFactoryRegistry);

        return this;
    }

    public ConfigQuantumInboxOutboxBuilder RegisterOutsideEventFactoryRegistry<T>(T registry)
        where T : class, IOutsideEventFactoryRegistry
    {
        collection.Collection.AddSingleton<IOutsideEventFactoryRegistry>(registry);

        return this;
    }

    public QuantumServiceCollection and()
    {
        return collection;
    }
}