using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.DataBase;
using Quantum.DataBase.EntityFramework;
using Quantum.InboxOutbox.Inbox.Configurator;
using Quantum.InboxOutbox.Inbox.Jobs;
using Quantum.InboxOutbox.Outbox.Configurator;
using Quartz;

namespace Quantum.InboxOutbox.Configuration;

public static class ConfigQuantumInboxOutboxExtensions
{
    public static ConfigQuantumInboxOutboxBuilder ConfigQuantumInboxOutbox(this QuantumServiceCollection collection)
    {
        return new ConfigQuantumInboxOutboxBuilder(collection);
    }
}

public class ConfigQuantumInboxOutboxBuilder(QuantumServiceCollection collection)
{
    public ConfigQuantumInboxOutboxBuilder ConfigureDefaults(
        DbContextOptions<QuantumDbContext> options, Assemblies assemblies, string topicName)
    {
        collection.Collection.Add(
            new ServiceDescriptor(typeof(InboxOutboxDbContext),
            typeof(InboxOutboxDbContext), ServiceLifetime.Transient));

        WithOptions(options);

        if (assemblies.InboxAssemblies.Any())
        {
            collection.ConfigQuantumInbox().ConfigureDefaults(topicName, assemblies.InboxAssemblies);
            collection.ConfigInboxJobs().ConfigureDefaults();
        }

        if (assemblies.OutboxAssemblies.Any())
        {
            collection.ConfigQuantumOutbox().ConfigureDefaults(topicName, assemblies.OutboxAssemblies);
            collection.ConfigOutboxJobs().ConfigureDefaults();
        }

        ConfigQuartzHostedService(collection);

        return this;
    }


    private void ConfigQuartzHostedService(QuantumServiceCollection quantumServiceCollection)
    {
        quantumServiceCollection.ConfigInboxJobs()
            .AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    private void WithOptions(DbContextOptions<QuantumDbContext> options)
    {
        collection.Collection.Add(new ServiceDescriptor(typeof(DbContextOptions<QuantumDbContext>), sp => options, ServiceLifetime.Singleton));
    }

    public QuantumServiceCollection and()
    {
        return collection;
    }
}