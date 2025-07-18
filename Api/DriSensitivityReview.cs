using System;
using System.Collections.Generic;

namespace Api;

public record DriSensitivityReview(string Id, string? TargetSubsetOrAssetReference, Uri TargetId,
    string AccessConditionCode, IEnumerable<Uri> Legislations, DateTimeOffset? ReviewDate = null, string? PreviousId = null,
    string? SensitiveName = null, string? SensitiveDescription = null, DateTimeOffset? Date = null, DateTimeOffset? RestrictionStartDate = null,
    long? RestrictionDuration = null, string? RestrictionDescription = null, long? InstrumentNumber = null, DateTimeOffset? InstrumentSignedDate = null,
    DateTimeOffset? RestrictionReviewDate = null, string? GroundForRetentionCode = null) : DriRecord(Id);
