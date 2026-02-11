using System;
using System.Collections.Generic;

namespace Api;

public record DriSensitivityReview(Uri Link, string TargetReference, Uri TargetLink, Uri TargetType,
    Uri? AccessCondition, IEnumerable<Uri> Legislations, DateTimeOffset? ReviewDate = null, Uri? PreviousLink = null,
    string? SensitiveName = null, string? SensitiveDescription = null, DateTimeOffset? Date = null, DateTimeOffset? RestrictionStartDate = null,
    long? RestrictionDuration = null, string? RestrictionDescription = null, long? InstrumentNumber = null, DateTimeOffset? InstrumentSignedDate = null,
    DateTimeOffset? RestrictionReviewDate = null, Uri? GroundForRetention = null,
    Uri? ChangeDriLink = null, string? ChangeDescription = null, DateTimeOffset? ChangeTimestamp = null,
    Uri? ChangeOperatorLink = null, string? ChangeOperatorName = null) : IDriRecord
{
    public string Id => Link.LastSegment();
    public string TargetId => TargetLink.LastSegment();
    public string? PreviousId => PreviousLink?.LastSegment();
    public string? ChangeId => ChangeDriLink?.LastSegment();
    public string? ChangeOperatorId => ChangeOperatorLink?.LastSegment();
}
