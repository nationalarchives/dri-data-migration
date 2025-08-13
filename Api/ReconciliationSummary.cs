namespace Api;

public class ReconciliationSummary
{
    public int AdditionalFilesCount { get; set; }
    public int AdditionalFolderCount { get; set; }
    public int MissingFilesCount { get; set; }
    public int MissingFolderCount { get; set; }
    public int DiffCount { get; set; }

    public ReconciliationSummary(int additionalFilesCount, int additionalFolderCount,
        int missingFilesCount, int missingFolderCount, int diffCount)
    {
        AdditionalFilesCount = additionalFilesCount;
        AdditionalFolderCount = additionalFolderCount;
        MissingFilesCount = missingFilesCount;
        MissingFolderCount = missingFolderCount;
        DiffCount = diffCount;
    }

    public void Update(ReconciliationSummary summary)
    {
        AdditionalFilesCount += summary.AdditionalFilesCount;
        AdditionalFolderCount += summary.AdditionalFolderCount;
        MissingFilesCount += summary.MissingFilesCount;
        MissingFolderCount += summary.MissingFolderCount;
        DiffCount += summary.DiffCount;
    }

    public bool HasDifference => AdditionalFilesCount > 0 || AdditionalFolderCount > 0 ||
        MissingFilesCount > 0 || MissingFolderCount > 0 || DiffCount > 0;
}
