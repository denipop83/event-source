using Wolverine.Http;
using Wolverine.Http.Marten;

namespace Queries.Features;

public static class GetApplicantInfo
{
    [WolverineGet("api/applicants/{id}")]
    public static IResult Handle(Guid id, [Document] Applicant? applicant) => Results.Ok(applicant);
}