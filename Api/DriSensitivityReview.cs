using System;
using System.Collections.Generic;
using System.Linq;

namespace Api;

public record DriSensitivityReview(Uri Link, string TargetReference, Uri TargetLink, Uri TargetType,
    Uri? AccessCondition, IEnumerable<Uri> Legislations, DateTimeOffset? ReviewDate = null, Uri? PreviousLink = null,
    string? SensitiveName = null, string? SensitiveDescription = null, DateTimeOffset? Date = null, DateTimeOffset? RestrictionStartDate = null,
    long? RestrictionDuration = null, string? RestrictionDescription = null, long? InstrumentNumber = null, DateTimeOffset? InstrumentSignedDate = null,
    DateTimeOffset? RestrictionReviewDate = null, Uri? GroundForRetention = null) : IDriRecord
{
    public string Id => Link.Segments.Last();
    public string TargetId => TargetLink.Segments.Last();
    public string? PreviousId => PreviousLink?.Segments.Last();
}
