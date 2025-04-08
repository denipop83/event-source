using Core;
using Marten;
using Wolverine.Persistence;

namespace Queries.Features;

public static class ApplicantUpdatedHandler
{
    public static Store<Applicant> Handle(ApplicantUpdated evt, [Entity(Required = false)] Applicant? aggregate)
    {
        aggregate ??= new Applicant { Id = evt.Id };

        aggregate.FirstName = evt.FirstName;
        aggregate.LastName = evt.LastName;
        aggregate.PassportSeries = evt.PassportSeries;
        aggregate.PassportNumber = evt.PassportNumber;

        return Storage.Store(aggregate);
    }
}

public static class IdentificationUpdatedHandler
{
    public static IStorageAction<Applicant> Handle(
        IdentificationUpdated evt,
        [Entity(Required = false)] Applicant? aggregate)
    {
        aggregate ??= new Applicant { Id = evt.ApplicantId, Identifications = [] };

        var identificationInfo = aggregate.Identifications.FirstOrDefault(x => x.IdentificationId == evt.Id);
        if (identificationInfo is null)
        {
            identificationInfo = new IdentificationInfo { IdentificationId = evt.Id };
            aggregate.Identifications.Add(identificationInfo);
        }

        var indexOf = aggregate.Identifications.IndexOf(identificationInfo);
        identificationInfo.Status = evt.Status;
        identificationInfo.UpdatedAt = DateTime.UtcNow;

        aggregate.Identifications[indexOf] = identificationInfo;
        aggregate.IdentificationsCount = aggregate.Identifications.Count;
        aggregate.LatestIdentificationStatus = evt.Status;

        return Storage.Store(aggregate);
    }
}