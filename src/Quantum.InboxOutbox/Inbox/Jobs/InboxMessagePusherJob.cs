using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Quantum.InboxOutbox.Inbox.Jobs;

public class InboxMessagePusherJob : IJob
{
    private readonly InboxOutboxDbContext _dbContext;
    private readonly IInboxMessageProcessor _inboxMessageProcessor;

    public InboxMessagePusherJob(IInboxMessageProcessor inboxMessageProcessor, InboxOutboxDbContext dbContext)
    {
        _inboxMessageProcessor = inboxMessageProcessor;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var inboxMessages = await _dbContext
            .InboxMessages.AsNoTracking()
            .Where(a => a.Seen == false)
            .OrderBy(a => a.OccurredOn)
            .Take(10)
            .ToListAsync();

        foreach (var message in inboxMessages) await _inboxMessageProcessor.Process(message);
    }
}