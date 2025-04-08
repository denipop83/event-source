namespace Queries;

public class Applicant
{
    public Guid Id { get; set; }
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public string PassportSeries { get; set; }
    public string PassportNumber { get; set; }

    public List<IdentificationInfo> Identifications { get; set; } = new();

    public int IdentificationsCount { get; set; }
    public string LatestIdentificationStatus { get; set; }
}

public class IdentificationInfo
{
    public Guid IdentificationId { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; }
}