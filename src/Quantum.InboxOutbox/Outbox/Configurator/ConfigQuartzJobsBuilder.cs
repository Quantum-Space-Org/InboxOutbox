using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.InboxOutbox.Outbox.Jobs;
using Quartz;

namespace Quantum.InboxOutbox.Outbox.Configurator;

public class ConfigQuartzJobsBuilder
{
    private readonly QuantumServiceCollection _collection;

    public ConfigQuartzJobsBuilder(QuantumServiceCollection collection)
    {
        _collection = collection;
    }

    public ConfigQuartzJobsBuilder ConfigureDefaults(int intervalInSeconds = 5)
    {
        var simpleScheduleBuilder = SimpleScheduleBuilder.Create()
            .WithIntervalInSeconds(intervalInSeconds).RepeatForever();

        RegisterAndConfigureQuartsJobs(collectionQuartzConfigurator =>
        {
            collectionQuartzConfigurator.UseMicrosoftDependencyInjectionJobFactory();

            var outboxMessagePusherJobKey = new JobKey(nameof(OutboxMessagePusherJob));

            collectionQuartzConfigurator.AddJob<OutboxMessagePusherJob>(
                options => options.WithIdentity(outboxMessagePusherJobKey)
            );

            collectionQuartzConfigurator.AddTrigger(options =>
                options.ForJob(outboxMessagePusherJobKey).WithIdentity($"{outboxMessagePusherJobKey.Name}-Trigger")
                    .StartNow()
                    .WithSimpleSchedule(simpleScheduleBuilder)
            );
        });

        AddQuartzHostedService(config => config.WaitForJobsToComplete = true);

        return this;
    }

    public ConfigQuartzJobsBuilder ConfigureQuartzOptions(Action<QuartzOptions> action)
    {
        _collection.Collection.Configure(action);

        return this;
    }


    public ConfigQuartzJobsBuilder RegisterAndConfigureQuartsJobs(Action<IServiceCollectionQuartzConfigurator> configure)
    {
        _collection.Collection.AddQuartz(configure);

        return this;
    }

    public ConfigQuartzJobsBuilder AddQuartzHostedService(Action<QuartzHostedServiceOptions> configure)
    {
        _collection.Collection.AddQuartzHostedService(configure);

        return this;
    }

    public QuantumServiceCollection and()
    {
        return _collection;
    }

}