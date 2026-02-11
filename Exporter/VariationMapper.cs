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
            var variationSizeBytes = graph.GetSingleNumber(variation, Vocabulary.VariationSizeBytes)!.Value;
            var checksums = GetChecksums(graph, variation);

            variations.Add(new()
            {
                FileId = variationDriId,
                FileName = variationName,
                SizeBytes = variationSizeBytes,
                Checksums = checksums.Any() ? checksums : null,
                SortOrder = variationSequence,
                Sequence = redactedVariationSequence,
                FilePath = variationRelativeLocation,
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

    private static List<RecordOutput.Checksum> GetChecksums(IGraph graph, IUriNode variation)
    {
        var checksums = new List<RecordOutput.Checksum>();
        foreach (var dataIntegrityId in graph.GetUriNodes(variation, Vocabulary.VariationHasVariationDataIntegrityCalculation))
        {
            var checksum = graph.GetSingleText(dataIntegrityId, Vocabulary.Checksum);
            var alg = graph.GetSingleUriNode(dataIntegrityId, Vocabulary.VariationDataIntegrityCalculationHasHashFunction);
            checksums.Add(new RecordOutput.Checksum()
            {
                Hash = alg?.Uri.LastSegment(),
                Value = checksum
            });
        }

        return checksums;
    }
}
