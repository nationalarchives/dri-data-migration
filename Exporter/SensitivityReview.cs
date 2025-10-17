using Api;
using System;
using System.Collections.Generic;

namespace Exporter;

internal class SensitivityReview
{
    public DateTimeOffset? FoiAssertedDate { get; set; }
    public string? SensitiveName { get; set; }
    public string? SensitiveDescription { get; set; }
    public string? AccessConditionName { get; set; }
    public string? AccessConditionCode { get; set; }
    public DateTimeOffset? ReviewDate { get; set; }
    public DateTimeOffset? ClosureStartDate { get; set; }
    public int? ClosurePeriod { get; set; }
    public int? EndYear { get; set; }
    public string? Description { get; set; }
    public IEnumerable<RecordOutput.Legislation>? FoiExemptions { get; set; }
    public long? InstrumentNumber { get; set; }
    public DateTimeOffset? InstrumentSignedDate { get; set; }
    public DateTimeOffset? RetentionReconsiderDate { get; set; }
    public string? GroundForRetentionCode { get; set; }
    public string? GroundForRetentionDescription { get; set; }
}
