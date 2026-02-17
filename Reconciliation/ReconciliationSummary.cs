namespace Reconciliation;

public class ReconciliationSummary
{
    private List<Diff> diffDetails { get; set; }
    public IReadOnlyCollection<Diff> DiffDetails => diffDetails;

    private List<string> missingFiles { get; set; }
    public IReadOnlyCollection<string> MissingFiles => missingFiles;

    private List<string> missingFolders { get; set; }
    public IReadOnlyCollection<string> MissingFolders => missingFolders;

    private List<string> additionalFiles { get; set; }
    public IReadOnlyCollection<string> AdditionalFiles => additionalFiles;

    private List<string> additionalFolders { get; set; }
    public IReadOnlyCollection<string> AdditionalFolders => additionalFolders;

    public ReconciliationSummary(List<Diff>? diffDetails = null,
        List<string>? missingFiles=null, List<string>? missingFolders = null,
        List<string>? additionalFiles = null, List<string>? additionalFolders = null)
    {
        this.diffDetails = diffDetails ?? [];
        this.missingFiles = missingFiles ?? [];
        this.missingFolders = missingFolders ?? [];
        this.additionalFiles = additionalFiles ?? [];
        this.additionalFolders = additionalFolders ?? [];
    }

    public void Update(ReconciliationSummary summary)
    {
        diffDetails.AddRange(summary.DiffDetails);
        missingFiles.AddRange(summary.MissingFiles);
        missingFolders.AddRange(summary.MissingFolders);
        additionalFiles.AddRange(summary.AdditionalFiles);
        additionalFolders.AddRange(summary.AdditionalFolders);
    }

    public bool HasDifference => AdditionalFiles.Count > 0 || AdditionalFolders.Count > 0 ||
        MissingFiles.Count > 0 || MissingFolders.Count > 0 || DiffDetails.Count > 0;

    public record Diff(string Id, List<DiffDetail> Details);

    public record DiffDetail(ReconciliationFieldName Field, object Expected, object Actual);
}
