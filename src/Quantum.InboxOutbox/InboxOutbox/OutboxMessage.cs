using System;
using System.ComponentModel.DataAnnotations;

namespace Quantum.InboxOutbox;

public class OutboxMessage
{
    private OutboxMessage() { }
    public OutboxMessage(string id,
        DateTime occurredOn,
        string type,
        string @event,
        bool publishToWorld)
    {
        Id = id;
        OccurredOn = occurredOn;
        Type = type;
        Event = @event;
        PublishToWorld = publishToWorld;
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
    public bool PublishToWorld { get; }

    public void SetSeenAt(DateTime dateTime)
        => SeenAt = dateTime;
    public void SetSeen()
        => Seen = true;
}