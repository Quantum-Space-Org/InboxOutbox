using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.DataBase;
using Quantum.DataBase.EntityFramework;

namespace Quantum.InboxOutbox.Configurator;

public static class ConfigQuantumInboxOutboxExtensions
{
    public static ConfigQuantumInboxOutboxBuilder ConfigQuantumInboxOutbox(this QuantumServiceCollection collection)
    {
        return new ConfigQuantumInboxOutboxBuilder(collection);
    }
}

public class ConfigQuantumInboxOutboxBuilder
{
    private readonly QuantumServiceCollection _collection;

    public ConfigQuantumInboxOutboxBuilder(QuantumServiceCollection collection)
        => _collection = collection;

    public ConfigQuantumInboxOutboxBuilder RegisterInboxOutboxDbContext<T>(DbContextOptions<QuantumDbContext> options
                , ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        _collection.Collection.Add(new ServiceDescriptor(typeof(InboxOutboxDbContext), typeof(T), lifetime));
        WithOptions(options);
        return this;
    }

    private ConfigQuantumInboxOutboxBuilder WithOptions(DbContextOptions<QuantumDbContext> options)
    {
        _collection.Collection.Add(new ServiceDescriptor(typeof(DbContextOptions<QuantumDbContext>), sp => options, ServiceLifetime.Singleton));

        return this;
    }

    public QuantumServiceCollection and()
    {
        return _collection;
    }
}