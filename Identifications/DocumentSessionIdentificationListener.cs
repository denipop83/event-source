﻿using System.Diagnostics;
using JasperFx.Core;
using Marten;
using Marten.Services;

namespace Identifications;

public class DocumentSessionIdentificationListener(IServiceScopeFactory serviceScopeFactory) : DocumentSessionListenerBase
{
    public override void BeforeSaveChanges(IDocumentSession session)
    {
        // Use pending changes to preview what is about to be
        // persisted
        var pending = session.PendingChanges;

        // Careful here, Marten can only sort documents into "inserts" or "updates" based
        // on whether or not Marten had to assign a new Id to that document upon DocumentStore()
        pending.InsertsFor<Identification>()
            .Each(user => Debug.WriteLine($"New user: {user.Status}"));

        pending.UpdatesFor<Identification>()
            .Each(user => Debug.WriteLine($"Updated user {user.Status}"));

        pending.DeletionsFor<Identification>()
            .Each(d => Debug.WriteLine(d));

        // This is a convenience method to find all the pending events
        // organized into streams that will be appended to the event store
        pending.Streams()
            .Each(s => Debug.WriteLine(s));
    }

    public override void AfterCommit(IDocumentSession session, IChangeSet commit)
    {
        // See what was just persisted, and possibly carry out post
        // commit actions

        var last = commit;

        last.Updated.Each(x => Debug.WriteLine($"{x} was updated"));
        last.Deleted.Each(x => Debug.WriteLine($"{x} was deleted"));
        last.Inserted.Each(x => Debug.WriteLine($"{x} was inserted"));
    }
}