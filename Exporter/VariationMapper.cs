using Api;
using Rdf;
using VDS.RDF;

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
            var variationRelativeLocation = graph.GetSingleText(variation, Vocabulary.VariationRelativeLocation);
            var scannerOperatorIdentifier = graph.GetSingleText(variation, Vocabulary.ScannerOperatorIdentifier);
            var scannerIdentifier = graph.GetSingleText(variation, Vocabulary.ScannerIdentifier);
            var scannerGeographicalPlace = graph.GetSingleTransitiveLiteral(variation, Vocabulary.ScannedVariationHasScannerGeographicalPlace, Vocabulary.GeographicalPlaceName)?.Value;
            var scannedVariationHasImageSplit = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageSplit);
            var scannedVariationHasImageCrop = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageCrop);
            var scannedVariationHasImageDeskew = graph.GetSingleUriNode(variation, Vocabulary.ScannedVariationHasImageDeskew);

            variations.Add(new()
            {
                FileId = variationDriId,
                FileName = variationName,
                SortOrder = variationSequence,
                Sequence = redactedVariationSequence,
                Location = variationRelativeLocation,
                ScannerOperatorIdentifier = scannerOperatorIdentifier,
                ScannerIdentifier = scannerIdentifier,
                ScannerGeographicalPlace = scannerGeographicalPlace,
                ScannedImageSplit = scannedVariationHasImageSplit?.Uri.Segments.LastOrDefault(),
                ScannedImageCrop = scannedVariationHasImageCrop?.Uri.Segments.LastOrDefault(),
                ScannedImageDeskew = scannedVariationHasImageDeskew?.Uri.Segments.LastOrDefault()
            });
        }

        return variations;
    }
}
