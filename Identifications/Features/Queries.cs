using Marten;
using Wolverine.Http;
using Wolverine.Http.Marten;

namespace Identifications.Features;

public static class Queries
{
    [WolverineGet("api/identifications/{id}")]
    public static IResult GetIdentification(Guid id, [Aggregate] Identification? identification) => Results.Ok(identification);
    
    [WolverineGet("api/identifications/{id}/timeline")]
    public static async Task<IResult> GetIdentificationTimeline(Guid id, IQuerySession querySession)
    {
        var events = await querySession.Events.QueryAllRawEvents()
            .Where(x => x.StreamId == id)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();
        
        var items = events.Select(
            x => new
            {
                Activity = x.EventTypeName,
                CreatedAt = x.Timestamp.UtcDateTime,
            }
        );
        
        return Results.Ok(items);
    }
}