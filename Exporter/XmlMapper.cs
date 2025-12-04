using Api;
using System.Text;
using VDS.RDF;

namespace Exporter;

internal static class XmlMapper
{
    public static List<XmlWrapper> Map(IGraph asset, IUriNode variation,
        long? redactedVariationSequence)
    {
        var wrappers = new List<XmlWrapper>();
        var assetReference = asset.GetSingleText(Vocabulary.AssetReference);
        var reference = ReferenceBuilder.Build(redactedVariationSequence, assetReference!);
        var assetDriXml = asset.GetSingleLiteral(Vocabulary.AssetDriXml)?.Value;
        if (assetDriXml is not null)
        {
            wrappers.Add(new(reference, FromBase64(assetDriXml)));
        }
        var wo409SubsetDriXml = asset.GetSingleLiteral(Vocabulary.Wo409SubsetDriXml)?.Value;
        if (wo409SubsetDriXml is not null)
        {
            wrappers.Add(new($"{reference}-WO-409", FromBase64(wo409SubsetDriXml)));
        }
        var variationDriXml = asset.GetSingleLiteral(variation, Vocabulary.VariationDriXml)?.Value;
        if (variationDriXml is not null)
        {
            var variationSequence = asset.GetSingleNumber(variation, Vocabulary.VariationSequence);
            var variationDriId = asset.GetSingleText(variation, Vocabulary.VariationDriId);
            var identifier = variationSequence is null ? variationDriId : variationSequence.Value.ToString();
            wrappers.Add(new($"{reference}-V-{identifier}", FromBase64(variationDriXml)));
        }

        return wrappers;
    }

    private static string FromBase64(string text) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(text));
}
