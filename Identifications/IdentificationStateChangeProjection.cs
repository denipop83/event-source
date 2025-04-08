using Core;
using Marten;
using Marten.Events.Daemon;
using Marten.Events.Daemon.Internals;
using Marten.Subscriptions;
using Wolverine;

namespace Identifications;

public class IdentificationChangeSubscription(IServiceScopeFactory serviceScopeFactory) : SubscriptionBase
{
    /// <inheritdoc />
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken)
    {
        
        using var scope = serviceScopeFactory.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        
        var tasks = page.Events
            .Select(x => x.StreamId)
            .Distinct()
            .ToDictionary(id => id, async x => await operations.Events.FetchLatest<Identification>(x, cancellationToken));

        await Task.WhenAll(tasks.Values);

        var publishTasks = tasks.Select(
                x => new IdentificationUpdated(x.Key, x.Value.Result.ApplicantId, x.Value.Result.Status))
            .Select(async x => await bus.PublishAsync(x));
        
        await Task.WhenAll(publishTasks);
        
        return NullChangeListener.Instance;
    }
}