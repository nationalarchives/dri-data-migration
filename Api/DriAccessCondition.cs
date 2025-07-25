using System;

namespace Api;

public record DriAccessCondition(Uri Link, string Name) : IDriRecord
{
    public string Id => Link.Fragment.TrimStart('#');
}
