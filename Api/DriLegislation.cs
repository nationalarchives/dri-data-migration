using System;

namespace Api;

public record DriLegislation(Uri Link, string? Section = null) : IDriRecord
{
    public string Id => Link.ToString();
}