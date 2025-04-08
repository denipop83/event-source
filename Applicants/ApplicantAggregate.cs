using Applicants.Events;

namespace Applicants;

public record ApplicantAggregate(string FirstName, string LastName, string PassportSeries, string PassportNumber)
{
    public static ApplicantAggregate Create(Registered registered) =>
        new(registered.FirstName, registered.LastName, registered.PassportSeries, registered.PassportNumber);
    
    public ApplicantAggregate Apply(PassportChanged passportChanged) => this with { PassportNumber = passportChanged.PassportNumber };
    public ApplicantAggregate Apply(NameChanged nameChanged) => this with { FirstName = nameChanged.FirstName, LastName = nameChanged.LastName };

    public Guid Id { get; set; }
}