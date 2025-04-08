using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace Identifications.Features;

public static class LinkOpenedHandler
{
    [WolverinePost("api/identifications/{id}/open_link")]
    public static (IResult, Events) Handle(Guid id, [Aggregate] Identification identification)
    {
        if (identification is Identification.WaitingApplicantActions)
        {
            return (Results.Accepted(), [
                new IdentificationEvent.LinkOpened(),
                new IdentificationEvent.CollectingData.WaitingForSelfie()
            ]);
        }

        return (Results.Accepted(), [new IdentificationEvent.LinkOpened()]);
    }
}