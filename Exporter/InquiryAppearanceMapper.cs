using Api;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Exporter;

internal static class InquiryAppearanceMapper
{
    internal static List<RecordOutput.InquiryAppearance>? GetInquiryAppearances(IGraph graph,
        IUriNode subject)
    {
        var appearance = graph.GetUriNodes(subject, Vocabulary.InquiryAssetHasInquiryAppearance);
        if (!appearance.Any())
        {
            return null;
        }

        var inquiries = new List<RecordOutput.InquiryAppearance>();
        foreach (var inquiry in appearance)
        {
            var inquiryWitnessName = graph.GetSingleText(inquiry, Vocabulary.InquiryWitnessName);
            var inquiryWitnessAppearanceDescription = graph.GetSingleText(inquiry, Vocabulary.InquiryWitnessAppearanceDescription);
            inquiries.Add(new()
            {
                WitnessName = inquiryWitnessName,
                AppearanceDescription = inquiryWitnessAppearanceDescription
            });
        }

        return inquiries;
    }
}