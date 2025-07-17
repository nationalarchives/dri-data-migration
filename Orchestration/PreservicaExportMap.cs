using Api;

namespace Orchestration;

public static class PreservicaExportMap
{
    public static Dictionary<string, ReconciliationRow> Full => new()
        {
            { "A", new(ReconciliationFieldNames.ImportLocation, PreservicaExportParser.ToLocation) },
            { "B", new(ReconciliationFieldNames.VariationName, PreservicaExportParser.ToText)},
            { "C", new(ReconciliationFieldNames.FileFolder, PreservicaExportParser.ToText)},
            { "N", new(ReconciliationFieldNames.AccessConditionName, PreservicaExportParser.ToText)},
            { "O", new(ReconciliationFieldNames.SensitivityReviewDuration, PreservicaExportParser.ToInt)},
            { "P", new(ReconciliationFieldNames.SensitivityReviewRestrictionCalculationStartDate, PreservicaExportParser.ToDate)},
            { "Q", new(ReconciliationFieldNames.LegislationSectionReference, PreservicaExportParser.ToText)},
            { "R", new(ReconciliationFieldNames.SensitivityReviewDate, PreservicaExportParser.ToDate)},
            { "S", new(ReconciliationFieldNames.IsPublicName, PreservicaExportParser.ToBool)},
            { "T", new(ReconciliationFieldNames.SensitivityReviewSensitiveName, PreservicaExportParser.ToText)},
            { "U", new(ReconciliationFieldNames.IsPublicDescription, PreservicaExportParser.ToBool)},
            { "V", new(ReconciliationFieldNames.SensitivityReviewSensitiveDescription, PreservicaExportParser.ToText)},
            { "W", new(ReconciliationFieldNames.RetentionType, PreservicaExportParser.ToText)},
            { "X", new(ReconciliationFieldNames.RetentionReviewDate, PreservicaExportParser.ToDate)},
            { "Y", new(ReconciliationFieldNames.RetentionInstrumentNumber, PreservicaExportParser.ToInt)},
            { "Z", new(ReconciliationFieldNames.RetentionInstrumentSignedDate, PreservicaExportParser.ToDate)},
            { "AA", new(ReconciliationFieldNames.GroundForRetentionCode, PreservicaExportParser.ToText)}
        };

    public static Dictionary<string, ReconciliationRow> Metadata => new()
        {
            { "identifier", new(ReconciliationFieldNames.ImportLocation, PreservicaExportParser.ToLocation) },
            { "file_name", new(ReconciliationFieldNames.VariationName, PreservicaExportParser.ToText)},
            { "folder", new(ReconciliationFieldNames.FileFolder, PreservicaExportParser.ToText)}
        };

    public static Dictionary<string, ReconciliationRow> Closure => new()
        {
            { "A", new(ReconciliationFieldNames.ImportLocation, PreservicaExportParser.ToLocation) },
            { "C", new(ReconciliationFieldNames.AccessConditionName, PreservicaExportParser.ToText)},
            { "D", new(ReconciliationFieldNames.SensitivityReviewDuration, PreservicaExportParser.ToInt)},
            { "E", new(ReconciliationFieldNames.SensitivityReviewRestrictionCalculationStartDate, PreservicaExportParser.ToDate)},
            { "F", new(ReconciliationFieldNames.LegislationSectionReference, PreservicaExportParser.ToTextList)},
            { "G", new(ReconciliationFieldNames.SensitivityReviewDate, PreservicaExportParser.ToDate)},
            { "H", new(ReconciliationFieldNames.IsPublicName, PreservicaExportParser.ToBool)},
            { "I", new(ReconciliationFieldNames.SensitivityReviewSensitiveName, PreservicaExportParser.ToText)}
        };
}

