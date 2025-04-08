using Core;
using Marten;
using Marten.Internal.Operations;
using Marten.Services;
using Wolverine;

namespace Identifications;

public class IdentificationChangeListener(IServiceScopeFactory serviceScopeFactory) : IChangeListener
{
    public Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        
        var uow = (IUnitOfWork)commit;

        var ops = uow.OperationsFor<Identification>();

        var publishTasks = ops
            .Select(x => (Identification)((IDocumentStorageOperation)x).Document)
            .Select(x => new IdentificationUpdated(x.Id, x.ApplicantId, x.Status))
            .Select(async x => await bus.PublishAsync(x));

        return Task.WhenAll(publishTasks);
    }

    public Task BeforeCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token) => Task.CompletedTask;
}