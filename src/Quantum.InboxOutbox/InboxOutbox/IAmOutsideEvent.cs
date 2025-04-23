using Quantum.Domain.Messages.Header;

namespace Quantum.InboxOutbox;

public abstract class IAmOutsideEvent
{
    public MessageMetadata Metadata { get; set; }
    protected IAmOutsideEvent(string type)
        => Metadata = new MessageMetadata("id", type);
}