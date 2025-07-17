using System;
using System.Collections.Generic;

namespace Api;

public record DriSensitivityReview(string Id, string? TargetSubsetOrAssetReference, Uri TargetId,
    string AccessConditionCode, IEnumerable<Uri> Legislations, DateTimeOffset? ReviewDate, string? PreviousId,
    string? SensitiveName, string? SensitiveDescription, DateTimeOffset? Date, DateTimeOffset? RestrictionStartDate,
    long? RestrictionDuration, string? RestrictionDescription, long? InstrumentNumber, DateTimeOffset? InstrumentSignedDate,
    DateTimeOffset? RestrictionReviewDate, string? GroundForRetentionCode) : DriRecord(Id);
