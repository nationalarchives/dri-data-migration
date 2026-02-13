namespace Reconciliation;

public class ReconciliationSummary
{
    public int AdditionalFilesCount { get; private set; }
    public int AdditionalFolderCount { get; private set; }
    public int MissingFilesCount { get; private set; }
    public int MissingFolderCount { get; private set; }
    public int DiffCount { get; private set; }
    
    private List<Diff> diffDetails { get; set; }
    public IReadOnlyCollection<Diff> DiffDetails => diffDetails;

    private List<string> missingFiles { get; set; }
    public IReadOnlyCollection<string> MissingFiles => missingFiles;

    private List<string> missingFolders { get; set; }
    public IReadOnlyCollection<string> MissingFolders => missingFolders;

    public ReconciliationSummary(int additionalFilesCount, int additionalFolderCount,
        int missingFilesCount, int missingFolderCount, int diffCount,
        List<Diff>? diffDetails = null, List<string>? missingFiles=null, List<string>? missingFolders = null)
    {
        AdditionalFilesCount = additionalFilesCount;
        AdditionalFolderCount = additionalFolderCount;
        MissingFilesCount = missingFilesCount;
        MissingFolderCount = missingFolderCount;
        DiffCount = diffCount;
        this.diffDetails = diffDetails ?? [];
        this.missingFiles = missingFiles ?? [];
        this.missingFolders = missingFolders ?? [];
    }

    public void Update(ReconciliationSummary summary)
    {
        AdditionalFilesCount += summary.AdditionalFilesCount;
        AdditionalFolderCount += summary.AdditionalFolderCount;
        MissingFilesCount += summary.MissingFilesCount;
        MissingFolderCount += summary.MissingFolderCount;
        DiffCount += summary.DiffCount;
        diffDetails.AddRange(summary.DiffDetails);
        missingFiles.AddRange(summary.MissingFiles);
        missingFolders.AddRange(summary.MissingFolders);
    }

    public bool HasDifference => AdditionalFilesCount > 0 || AdditionalFolderCount > 0 ||
        MissingFilesCount > 0 || MissingFolderCount > 0 || DiffCount > 0;

    public record Diff(string Id, List<DiffDetail> Details);

    public record DiffDetail(ReconciliationFieldName Field, object Expected, object Actual);
}
