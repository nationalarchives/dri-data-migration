using Api;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

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
            var redactedVariationSequence = graph.GetSingleNumber(variation, Vocabulary.RedactedVariationSequence);
            var variationNote = graph.GetSingleText(variation, Vocabulary.VariationNote);
            var variationRelativeLocation = graph.GetSingleText(variation, Vocabulary.VariationRelativeLocation);
            var variationPhysicalConditionDescription = graph.GetSingleText(variation, Vocabulary.VariationPhysicalConditionDescription);
            var variationReferenceGoogleId = graph.GetSingleText(variation, Vocabulary.VariationReferenceGoogleId);
            var variationReferenceParentGoogleId = graph.GetSingleText(variation, Vocabulary.VariationReferenceParentGoogleId);
            var scannerOperatorIdentifier = graph.GetSingleText(variation, Vocabulary.ScannerOperatorIdentifier);
            var scannerIdentifier = graph.GetSingleText(variation, Vocabulary.ScannerIdentifier);
            var archivistNote = graph.GetSingleTransitiveLiteral(variation, Vocabulary.VariationHasDatedNote, Vocabulary.ArchivistNote)?.Value;
            var scannerGeographicalPlace = graph.GetSingleTransitiveLiteral(variation, Vocabulary.ScannedVariationHasScannerGeographicalPlace, Vocabulary.GeographicalPlaceName)?.Value;
            var scannedVariationHasImageSplit = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageSplit);
            var scannedVariationHasImageCrop = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageCrop);
            var scannedVariationHasImageDeskew = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageDeskew);

            string? noteDate = null;
            var variationHasDatedNote = graph.GetSingleUriNode(variation, Vocabulary.VariationHasDatedNote);
            if (variationHasDatedNote is not null)
            {
                noteDate = YmdMapper.GetYmd(graph, variationHasDatedNote, Vocabulary.DatedNoteHasDate);
            }


            variations.Add(new()
            {
                FileName = variationName,
                //TODO: Add SortOrder=,
                RedactionSequence = redactedVariationSequence,
                Note = variationNote,
                Location = variationRelativeLocation,
                PhysicalConditionDescription = variationPhysicalConditionDescription,
                ReferenceGoogleId = variationReferenceGoogleId,
                ReferenceParentGoogleId = variationReferenceParentGoogleId,
                ScannerOperatorIdentifier = scannerOperatorIdentifier,
                ScannerIdentifier = scannerIdentifier,
                ArchivistNote = archivistNote,
                ArchivistNoteDate = noteDate,
                ScannerGeographicalPlace = scannerGeographicalPlace,
                ScannedImageSplit = scannedVariationHasImageSplit?.Uri.Segments.LastOrDefault(),
                ScannedImageCrop = scannedVariationHasImageCrop?.Uri.Segments.LastOrDefault(),
                ScannedImageDeskew = scannedVariationHasImageDeskew?.Uri.Segments.LastOrDefault()
            });
        }

        return variations;
    }
}