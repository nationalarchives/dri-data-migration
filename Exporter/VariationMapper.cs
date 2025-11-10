using Api;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static class VariationMapper
{
    internal static List<RecordOutput.Variation> GetVariations(IGraph graph,
        List<IUriNode> variationSubjects)
    {
        var variations = new List<RecordOutput.Variation>();
        foreach (var variation in variationSubjects)
        {
            var variationName = graph.GetSingleText(variation, Vocabulary.VariationName);
            var variationSequence = graph.GetSingleNumber(variation, Vocabulary.VariationSequence);
            var redactedVariationSequence = graph.GetSingleNumber(variation, Vocabulary.RedactedVariationSequence);
            var variationNote = graph.GetSingleText(variation, Vocabulary.VariationNote);
            var variationRelativeLocation = graph.GetSingleText(variation, Vocabulary.VariationRelativeLocation);
            var variationPhysicalConditionDescription = graph.GetSingleText(variation, Vocabulary.VariationPhysicalConditionDescription);
            var variationReferenceGoogleId = graph.GetSingleText(variation, Vocabulary.VariationReferenceGoogleId);
            var variationReferenceParentGoogleId = graph.GetSingleText(variation, Vocabulary.VariationReferenceParentGoogleId);
            var scannerOperatorIdentifier = graph.GetSingleText(variation, Vocabulary.ScannerOperatorIdentifier);
            var scannerIdentifier = graph.GetSingleText(variation, Vocabulary.ScannerIdentifier);
            var scannerGeographicalPlace = graph.GetSingleTransitiveLiteral(variation, Vocabulary.ScannedVariationHasScannerGeographicalPlace, Vocabulary.GeographicalPlaceName)?.Value;
            var scannedVariationHasImageSplit = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageSplit);
            var scannedVariationHasImageCrop = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageCrop);
            var scannedVariationHasImageDeskew = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageDeskew);

            var archivistNotes = new List<RecordOutput.ArchivistNote>();
            foreach (var datedNote in graph.GetUriNodes(variation, Vocabulary.VariationHasDatedNote))
            {
                var archivistNote = graph.GetSingleText(datedNote, Vocabulary.ArchivistNote);
                var noteDate = graph.GetSingleLiteral(datedNote, Vocabulary.ArchivistNoteAt);
                if (noteDate is not null)
                {
                    archivistNotes.Add(new RecordOutput.ArchivistNote()
                    {
                        Note = archivistNote,
                        Date = noteDate.AsValuedNode().AsDateTimeOffset().ToString("yyyy-MM-dd HH:mm:ssK")
                    });
                }
                else
                {
                    archivistNotes.Add(new RecordOutput.ArchivistNote()
                    {
                        Note = archivistNote,
                        Date = YmdMapper.GetYmd(graph, datedNote, Vocabulary.DatedNoteHasDate)
                    });
                }
            }

            variations.Add(new()
            {
                FileName = variationName,
                SortOrder = variationSequence,
                RedactionSequence = redactedVariationSequence,
                Note = variationNote,
                Location = variationRelativeLocation,
                PhysicalConditionDescription = variationPhysicalConditionDescription,
                ReferenceGoogleId = variationReferenceGoogleId,
                ReferenceParentGoogleId = variationReferenceParentGoogleId,
                ScannerOperatorIdentifier = scannerOperatorIdentifier,
                ScannerIdentifier = scannerIdentifier,
                ArchivistNotes = archivistNotes.Count == 0 ? null : archivistNotes,
                ScannerGeographicalPlace = scannerGeographicalPlace,
                ScannedImageSplit = scannedVariationHasImageSplit?.Uri.Segments.LastOrDefault(),
                ScannedImageCrop = scannedVariationHasImageCrop?.Uri.Segments.LastOrDefault(),
                ScannedImageDeskew = scannedVariationHasImageDeskew?.Uri.Segments.LastOrDefault()
            });
        }

        return variations;
    }
}