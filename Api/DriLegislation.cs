using System;

namespace Api;

public record DriLegislation(Uri Link, string? Section = null) : DriRecord(Link.ToString());