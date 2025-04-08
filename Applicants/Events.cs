namespace Applicants.Events;

public record Registered(string FirstName, string LastName, string PassportSeries, string PassportNumber);
public record PassportChanged(string PassportSeries, string PassportNumber);
public record NameChanged(string FirstName, string LastName);