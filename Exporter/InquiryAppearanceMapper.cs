using Api;
using Rdf;
using VDS.RDF;

namespace Exporter;

internal static class InquiryAppearanceMapper
{
    internal static List<RecordOutput.InquiryAppearance>? GetInquiryAppearances(IGraph graph)
    {
        var appearance = graph.GetUriNodes(Vocabulary.InquiryAssetHasInquiryAppearance);
        if (!appearance.Any())
        {
            return null;
        }

        var inquiries = new List<RecordOutput.InquiryAppearance>();
        foreach (var inquiry in appearance)
        {
            var inquiryAppearanceSequence = graph.GetSingleNumber(inquiry, Vocabulary.InquiryAppearanceSequence);
            var inquiryWitnessNames = graph.GetUriNodes(inquiry, Vocabulary.InquiryAppearanceHasInquiryWitness)
                .Select(w => graph.GetSingleText(w, Vocabulary.InquiryWitnessName)!);
            var inquiryWitnessAppearanceDescription = graph.GetSingleText(inquiry, Vocabulary.InquiryWitnessAppearanceDescription);
            inquiries.Add(new()
            {
                Sequence = inquiryAppearanceSequence,
                WitnessNames = inquiryWitnessNames.Any() ? inquiryWitnessNames : null,
                AppearanceDescription = inquiryWitnessAppearanceDescription
            });
        }

        return inquiries;
    }
}