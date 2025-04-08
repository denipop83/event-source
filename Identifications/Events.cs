using Wolverine.Persistence.Sagas;

namespace Identifications;

public abstract record IdentificationEvent
{
    
    [SagaIdentity]
    public Guid Id { get; init; }
    public Guid ApplicantId { get; init; }

    public record Registered : IdentificationEvent;
    public record LinkOpened : IdentificationEvent;

    public abstract record CollectingData : IdentificationEvent
    {
        public record WaitingForSelfie : CollectingData;
        public record SelfieReceived : CollectingData;
        public record WaitingForAddress : CollectingData;
        public record AddressReceived : CollectingData;
        public record WaitingForPassport : CollectingData;
        public record PassportReceived : CollectingData;
    }

    public record CollectingDataFailed : IdentificationEvent;

    public record ProcessingDataStarted(DateTimeOffset StartedProcessingDataAt) : IdentificationEvent;

    public record Succeeded : IdentificationEvent;

    public record Failed : IdentificationEvent;
}