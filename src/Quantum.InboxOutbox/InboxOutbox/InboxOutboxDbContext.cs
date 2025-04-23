using Microsoft.EntityFrameworkCore;
using Quantum.DataBase;
using Quantum.DataBase.EntityFramework;

namespace Quantum.InboxOutbox;

public class InboxOutboxDbContext : QuantumDbContext
{
    public InboxOutboxDbContext(DbContextOptionsBuilder<QuantumDbContext> options)
        : base(new DbContextConfig(options))
    {
    }

    public DbSet<InboxMessage> InboxMessages { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<OutboxMessage>()
            .HasKey(a => a.Id);

        base.OnModelCreating(modelBuilder);
    }
}