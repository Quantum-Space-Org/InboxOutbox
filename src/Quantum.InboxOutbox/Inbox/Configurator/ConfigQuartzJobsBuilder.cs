using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.InboxOutbox.Inbox.Jobs;
using Quartz;

namespace Quantum.InboxOutbox.Inbox.Configurator;

public class ConfigQuartzJobsBuilder
{
    private readonly QuantumServiceCollection _collection;

    public ConfigQuartzJobsBuilder(QuantumServiceCollection collection)
    {
        this._collection = collection;
    }

    public ConfigQuartzJobsBuilder ConfigureDefaults(int intervalInSeconds = 5)
    {
        var simpleScheduleBuilder = SimpleScheduleBuilder.Create()
            .WithIntervalInSeconds(intervalInSeconds).RepeatForever();

        RegisterAndConfigureQuartsJobs(collectionQuartzConfigurator =>
        {
            collectionQuartzConfigurator.UseMicrosoftDependencyInjectionJobFactory();

            var inboxMessagePusherJobKey = new JobKey(nameof(InboxMessagePusherJob));

            collectionQuartzConfigurator.AddJob<InboxMessagePusherJob>(
                options => options.WithIdentity(inboxMessagePusherJobKey)
            );

            collectionQuartzConfigurator.AddTrigger(options =>
                options.ForJob(inboxMessagePusherJobKey)
                    .WithIdentity($"{inboxMessagePusherJobKey.Name}-Trigger")
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

    public QuantumServiceCollection and()
    {
        return _collection;
    }

    public ConfigQuartzJobsBuilder RegisterAndConfigureQuartsJobs(Action<IServiceCollectionQuartzConfigurator>? configure)
    {
        _collection.Collection.AddQuartz(configure);

        return this;
    }

    public ConfigQuartzJobsBuilder AddQuartzHostedService(Action<QuartzHostedServiceOptions>? configure)
    {
        _collection.Collection.AddQuartzHostedService(configure);

        return this;
    }
}
