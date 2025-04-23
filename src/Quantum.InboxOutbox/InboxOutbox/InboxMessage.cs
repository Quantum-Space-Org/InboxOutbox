using System;
using System.ComponentModel.DataAnnotations;

namespace Quantum.InboxOutbox;

public class InboxMessage
{
    private InboxMessage() { }
    public InboxMessage(string id,
        DateTime occurredOn,
        string type,
        string @event)
    {
        Id = id;
        OccurredOn = occurredOn;
        Type = type;
        Event = @event;
        SeenAt = null;
        Seen = false;
    }

    [Key]
    public string Id { get; }

    public DateTime OccurredOn { get; }

    public string Event { get; }

    public string Type { get; }

    public DateTime? SeenAt { get; private set; }

    public bool Seen { get; private set; }

    public void SetSeenAt(DateTime utcNow)
    {
        SeenAt = utcNow;
    }

    public void SetSeen()
    {
        Seen = true;
    }
}