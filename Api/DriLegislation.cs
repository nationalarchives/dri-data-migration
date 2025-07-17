using System;

namespace Api;

public record DriLegislation(Uri Link, string? Section) : DriRecord(Link.ToString());