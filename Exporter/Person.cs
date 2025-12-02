namespace Exporter;

internal class Person
{
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? DateOfBirth { get; set; }
    public string? BirthAddress { get; set; }
    public string? NationalRegistrationNumber { get; set; }
    public string? SeamanServiceNumber { get; set; }
    public string? BattalionName { get; set; }
    public string? NextOfKinName { get; set; }
    public IEnumerable<string>? NextOfKinTypes { get; set; }
    public bool? IsVeteran { get; set; }
}
