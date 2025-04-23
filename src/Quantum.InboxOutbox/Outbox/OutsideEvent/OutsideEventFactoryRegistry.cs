using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Quantum.InboxOutbox.Outbox.OutsideEvent;

public class OutsideEventFactoryRegistry : IOutsideEventFactoryRegistry
{
    private readonly Dictionary<Type, Type> _registry;
    public static OutsideEventFactoryRegistryBuilder Builder => new();

    public OutsideEventFactoryRegistry(Dictionary<Type, Type> registry)
    {
        _registry = registry;
    }
    
    public Core.Nullable<IOutsideEventFactory<T>> Resolve<T>() where T : DomainEvent
    {
        var outsideEventFactory = _registry != null && _registry.TryGetValue(typeof(T), out var result)
            ? (IOutsideEventFactory<T>)result
            : null;
        
        return Core.Nullable<IOutsideEventFactory<T>>.Instance(outsideEventFactory != null ? Activator.CreateInstance(outsideEventFactory.GetType()) :null);
    }

    public Core.Nullable<object> Resolve(Type type)
    {
        var outsideEventFactory = _registry != null && _registry.TryGetValue(type, out var result)
            ? result
            : null;

        return Core.Nullable<object>.Instance(outsideEventFactory!=null ? Activator.CreateInstance(outsideEventFactory.GetType()) : null);
    }
}

public class OutsideEventFactoryRegistryBuilder
{
    private readonly Dictionary<Type, Type> _registry = new();


    public OutsideEventFactoryRegistryBuilder Register<TDomainEvent, TFactory>()
    {
        _registry[typeof(TDomainEvent)] = typeof(TFactory);
        return this;
    }

    public OutsideEventFactoryRegistryBuilder Register(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t =>
            t.IsClass &&
            t.GetInterfaces().Any(i =>
                i.GetInterfaces() != null
                && i.Name.StartsWith("IOutsideEventFactory")));

        foreach (var type in types)
        {
            var inter = type.GetInterfaces()[0];
            _registry[type] = inter;
        }
        return this;
    }

    public OutsideEventFactoryRegistryBuilder Register(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            Register(assembly);
        }

        return this;
    }

    public OutsideEventFactoryRegistry Build()
    {
        return new OutsideEventFactoryRegistry(_registry);
    }
}