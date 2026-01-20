using Api;
using Rdf;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static class VariationMapper
{
    internal static List<RecordOutput.Variation> GetVariations(IGraph graph, List<IUriNode> variationSubjects)
    {
        var variations = new List<RecordOutput.Variation>();
        foreach (var variation in variationSubjects)
        {
            var variationDriId = graph.GetSingleText(variation, Vocabulary.VariationDriId)!;
            var variationName = graph.GetSingleText(variation, Vocabulary.VariationName)!;
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
            var datedNote = graph.GetSingleUriNode(variation, Vocabulary.VariationHasDatedNote);
            string? archivistNote = null;
            string? archivistNoteDate = null;
            if (datedNote is not null)
            {
                archivistNote = graph.GetSingleText(datedNote, Vocabulary.ArchivistNote);
                var noteDate = graph.GetSingleLiteral(datedNote, Vocabulary.ArchivistNoteAt);
                if (noteDate is not null)
                {
                    archivistNoteDate = noteDate.AsValuedNode().AsDateTimeOffset().ToString("O");
                }
                else
                {
                    archivistNoteDate = YmdMapper.GetYmd(graph, datedNote, Vocabulary.DatedNoteHasDate);
                }
            }

            variations.Add(new()
            {
                FileId = variationDriId,
                FileName = variationName,
                SortOrder = variationSequence,
                Sequence = redactedVariationSequence,
                Note = variationNote,
                Location = variationRelativeLocation,
                PhysicalConditionDescription = variationPhysicalConditionDescription,
                ReferenceGoogleId = variationReferenceGoogleId,
                ReferenceParentGoogleId = variationReferenceParentGoogleId,
                ScannerOperatorIdentifier = scannerOperatorIdentifier,
                ScannerIdentifier = scannerIdentifier,
                ArchivistNote = archivistNote,
                ArchivistNoteDate = archivistNoteDate,
                ScannerGeographicalPlace = scannerGeographicalPlace,
                ScannedImageSplit = scannedVariationHasImageSplit?.Uri.Segments.LastOrDefault(),
                ScannedImageCrop = scannedVariationHasImageCrop?.Uri.Segments.LastOrDefault(),
                ScannedImageDeskew = scannedVariationHasImageDeskew?.Uri.Segments.LastOrDefault()
            });
        }

        return variations;
    }
}
