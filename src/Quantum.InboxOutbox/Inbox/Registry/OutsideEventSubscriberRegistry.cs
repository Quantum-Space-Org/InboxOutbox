using Quantum.Core.Exceptions;
using Quantum.Resolver;

namespace Quantum.InboxOutbox.Inbox.Registry;

public class OutsideEventSubscriberRegistry : IOutsideEventSubscriberRegistry
{
    private readonly IResolver _resolver;

    public OutsideEventSubscriberRegistry(IResolver resolver) => _resolver = resolver;

    public Core.Nullable<object> Resolve(Type type)
    {
        var subscriberType = Type.MakeGenericSignatureType(typeof(Subscriber.IWantToSubscribeToOutsideEvent<>), type);

        try
        {
            var resolve = _resolver.Resolve(subscriberType);
            return Core.Nullable<object>.Instance(resolve);
        }
        catch (QuantumComponentIsNotRegisteredException exception)
        {
            return Core.Nullable<object>.Instance(null);
        }
    }
}