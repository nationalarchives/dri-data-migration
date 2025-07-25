namespace Api;

public record DriGroundForRetention(string Code, string Description) : IDriRecord
{
    public string Id => Code;
}
