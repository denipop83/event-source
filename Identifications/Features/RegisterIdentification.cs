using Wolverine.Attributes;
using Wolverine.Marten;

namespace Identifications.Features;


[WolverineHandler]
public static class RegisterIdentification
{
    public record Command(Guid IdentificationId, Guid ApplicantId);

    public static IStartStream Handle(Command registerIdentification) =>
        MartenOps.StartStream<Identification>(
            registerIdentification.IdentificationId,
            new IdentificationEvent.Registered
            {
                Id = registerIdentification.IdentificationId,
                ApplicantId = registerIdentification.ApplicantId
            }
        );
}