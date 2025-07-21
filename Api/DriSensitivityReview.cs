using System;
using System.Collections.Generic;

namespace Api;

public record DriSensitivityReview(Uri Link, string TargetReference, Uri TargetId, Uri TargetType,
    Uri AccessCondition, IEnumerable<Uri> Legislations, DateTimeOffset? ReviewDate = null, Uri? PreviousId = null,
    string? SensitiveName = null, string? SensitiveDescription = null, DateTimeOffset? Date = null, DateTimeOffset? RestrictionStartDate = null,
    long? RestrictionDuration = null, string? RestrictionDescription = null, long? InstrumentNumber = null, DateTimeOffset? InstrumentSignedDate = null,
    DateTimeOffset? RestrictionReviewDate = null, Uri? GroundForRetention = null) : DriRecord(Link.ToString());
