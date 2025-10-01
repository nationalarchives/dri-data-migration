namespace Reconciliation;

public class DiscoverySearchRecordsResponse
{
    public Record[] Records { get; set; }
    public int Count { get; set; }
    public string NextBatchMark { get; set; }

    public class Record
    {
        public string? AltName { get; set; }
        public string[] Places { get; set; }
        public string[] CorpBodies { get; set; }
        public string[] Taxonomies { get; set; }
        public string? FormerReferenceDep { get; set; }
        public string? FormerReferencePro { get; set; }
        public string[] HeldBy { get; set; }
        public string? Context { get; set; }
        public string? Content { get; set; }
        public string? URLParameters { get; set; }
        public string? Department { get; set; }
        public string? Note { get; set; }
        public string? AdminHistory { get; set; }
        public string? Arrangement { get; set; }
        public string? MapDesignation { get; set; }
        public string? MapScale { get; set; }
        public string? PhysicalCondition { get; set; }
        public int? CatalogueLevel { get; set; }
        public string? OpeningDate { get; set; }
        public string? ClosureStatus { get; set; }
        public string? ClosureType { get; set; }
        public string? ClosureCode { get; set; }
        public string? DocumentType { get; set; }
        public string? CoveringDates { get; set; }
        public string? Description { get; set; }
        public string? EndDate { get; set; }
        public int? NumEndDate { get; set; }
        public int? NumStartDate { get; set; }
        public string? StartDate { get; set; }
        public string? Id { get; set; }
        public string? Reference { get; set; }
        public double? Score { get; set; }
        public string? Source { get; set; }
        public string? Title { get; set; }
    }
}
