using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Quantum.InboxOutbox.Outbox.Jobs;

public class OutboxMessagePusherJob : IJob
{
    private readonly InboxOutboxDbContext _dbContext;
    private readonly IOutBoxMessagePublisher _outBoxMessagePublisher;

    public OutboxMessagePusherJob(IOutBoxMessagePublisher outBoxMessagePublisher, InboxOutboxDbContext dbContext)
    {
        _outBoxMessagePublisher = outBoxMessagePublisher;
        _dbContext = dbContext;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var outboxMessages = await _dbContext
            .OutboxMessages.AsNoTracking()
            .Where(a => a.Seen == false)
            .OrderBy(a => a.OccurredOn)
            .Take(10)
            .ToListAsync();

        foreach (var message in outboxMessages) 
            await _outBoxMessagePublisher.Publish(message);
    }
}