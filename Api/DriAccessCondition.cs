using System;

namespace Api;

public record DriAccessCondition(Uri Link, string Name)
    : DriRecord(Link.ToString());
