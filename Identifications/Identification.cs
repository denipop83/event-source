namespace Identifications;

public abstract record Identification
{
    public Guid Id { get; init; }

    public string Status => GetType().Name;

    public Guid ApplicantId { get; init; }

    public record WaitingApplicantActions : Identification;

    public record CollectingData : Identification
    {
        public record WaitingDocument : CollectingData;

        public record WaitingSelfie : CollectingData
        {
            public int Count { get; init; }
        }

        public record WaitingAddress : CollectingData;
    }

    public record InProgress : Identification;

    public record Succeeded : Identification;

    public record Failed : Identification;

    public static Identification Create(IdentificationEvent.Registered registered) =>
        new WaitingApplicantActions { ApplicantId = registered.ApplicantId };

    public Identification Apply(IdentificationEvent.CollectingData.WaitingForSelfie _) =>
        this is WaitingApplicantActions ? new CollectingData.WaitingSelfie { ApplicantId = ApplicantId } : this;

    public Identification Apply(IdentificationEvent.CollectingData.WaitingForPassport _) =>
        this is WaitingApplicantActions ? new CollectingData.WaitingDocument { ApplicantId = ApplicantId } : this;

    public Identification Apply(IdentificationEvent.CollectingData.WaitingForAddress _) =>
        this is WaitingApplicantActions ? new CollectingData.WaitingAddress { ApplicantId = ApplicantId } : this;

    public Identification Apply(IdentificationEvent.ProcessingDataStarted _) =>
        this is CollectingData ? new InProgress() { ApplicantId = ApplicantId } : this;

    public Identification Apply(IdentificationEvent.Succeeded _) => this is InProgress ? new Succeeded { ApplicantId = ApplicantId } : this;

    public Identification Apply(IdentificationEvent.Failed _) =>
        this is CollectingData or InProgress ? new Failed { ApplicantId = ApplicantId } : this;

    private Identification()
    {
    }
}