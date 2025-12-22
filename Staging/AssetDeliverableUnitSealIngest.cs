using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using VDS.RDF;

namespace Staging;

internal class AssetDeliverableUnitSealIngest(ILogger logger, ICacheClient cacheClient)
{
    private readonly DimensionParser dimensionParser = new(logger);
    private readonly DateParser dateParser = new(logger);

    internal async Task AddSealAsync(IGraph graph, IGraph rdf, IGraph existing, IUriNode id, CancellationToken cancellationToken)
    {
        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, id, rdf,
            IngestVocabulary.TypeOfSeal, CacheEntityKind.SealCategory,
            Vocabulary.SealAssetHasSealCategory, Vocabulary.SealCategoryName, cancellationToken);

        var obverseOrReverse = rdf.GetTriplesWithPredicate(IngestVocabulary.Face).SingleOrDefault()?.Object as ILiteralNode;

        var dateNode = rdf.GetTriplesWithPredicate(IngestVocabulary.DateOfOriginalSeal).FirstOrDefault()?.Object as ILiteralNode;
        if (dateNode is not null && !string.IsNullOrWhiteSpace(dateNode.Value))
        {
            var range = dateParser.ParseDateRange(obverseOrReverse?.Value, dateNode.Value);
            switch (range.DateRangeKind)
            {
                case DateParser.DateRangeType.Date:
                    AssertSealDate(graph, existing, id, Vocabulary.SealAssetHasStartDate,
                        Vocabulary.SealAssetHasEndDate, range);
                    break;
                case DateParser.DateRangeType.IdenticalObverseAndReverse:
                    AssertSealDate(graph, existing, id, Vocabulary.SealAssetHasObverseStartDate,
                        Vocabulary.SealAssetHasObverseEndDate, range);
                    AssertSealDate(graph, existing, id, Vocabulary.SealAssetHasReverseStartDate,
                        Vocabulary.SealAssetHasReverseEndDate, range);
                    break;
                case DateParser.DateRangeType.Obverse:
                    AssertSealDate(graph, existing, id, Vocabulary.SealAssetHasObverseStartDate,
                        Vocabulary.SealAssetHasObverseEndDate, range);
                    break;
                case DateParser.DateRangeType.Reverse:
                    AssertSealDate(graph, existing, id, Vocabulary.SealAssetHasReverseStartDate,
                        Vocabulary.SealAssetHasReverseEndDate, range);
                    break;
            }
        }
        var dimensionNode = rdf.GetTriplesWithPredicate(IngestVocabulary.Dimensions).SingleOrDefault()?.Object as ILiteralNode;
        if (dimensionNode is not null && !string.IsNullOrWhiteSpace(dimensionNode.Value))
        {
            var dimension = dimensionParser.ParseCentimetre(obverseOrReverse?.Value, dimensionNode.Value);
            switch (dimension.DimensionKind)
            {
                case DimensionParser.DimensionType.Fragment:
                    graph.Assert(id, Vocabulary.AssetHasDimension, Vocabulary.FragmentDimension);
                    break;
                case DimensionParser.DimensionType.ObverseFragment:
                    graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, Vocabulary.FragmentDimension);
                    break;
                case DimensionParser.DimensionType.ReverseFragment:
                    graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, Vocabulary.FragmentDimension);
                    break;
                case DimensionParser.DimensionType.ObverseAndReverseFragment:
                    graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, Vocabulary.FragmentDimension);
                    graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, Vocabulary.FragmentDimension);
                    break;
                case DimensionParser.DimensionType.Obverse:
                    AssertFirstDimension(graph, existing, id, Vocabulary.SealAssetHasObverseDimension, dimension);
                    break;
                case DimensionParser.DimensionType.Reverse:
                    AssertFirstDimension(graph, existing, id, Vocabulary.SealAssetHasReverseDimension, dimension);
                    break;
                case DimensionParser.DimensionType.Dimension:
                    AssertFirstDimension(graph, existing, id, Vocabulary.AssetHasDimension, dimension);
                    break;
                case DimensionParser.DimensionType.FragmentObverseSecondReverse:
                    graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, Vocabulary.FragmentDimension);
                    AssertSecondDimension(graph, existing, id, Vocabulary.SealAssetHasReverseDimension, dimension);
                    break;
                case DimensionParser.DimensionType.FirstObverseFragmentReverse:
                    AssertFirstDimension(graph, existing, id, Vocabulary.SealAssetHasObverseDimension, dimension);
                    graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, Vocabulary.FragmentDimension);
                    break;
                case DimensionParser.DimensionType.IdenticalObverseAndReverse:
                case DimensionParser.DimensionType.FirstObverseSecondReverse:
                    AssertFirstDimension(graph, existing, id, Vocabulary.SealAssetHasObverseDimension, dimension);
                    AssertSecondDimension(graph, existing, id, Vocabulary.SealAssetHasReverseDimension, dimension);
                    break;
            }
        }
    }

    private static void AssertSealDate(IGraph graph, IGraph existing, IUriNode id,
        IUriNode startDatePredicate, IUriNode endDatePredicate, DateParser.DateRange range)
    {
        var startNode = existing.GetSingleUriNode(id, startDatePredicate) ?? CacheClient.NewId;
        graph.Assert(id, startDatePredicate, startNode);
        GraphAssert.YearMonthDay(graph, startNode, range.FirstYear, range.FirstMonth, range.FirstDay);
        if (range.SecondYear.HasValue)
        {
            var endNode = existing.GetSingleUriNode(id, endDatePredicate) ?? CacheClient.NewId;
            graph.Assert(id, endDatePredicate, endNode);
            GraphAssert.YearMonthDay(graph, endNode, range.SecondYear, range.SecondMonth, range.SecondDay);
        }
    }

    private static void AssertFirstDimension(IGraph graph, IGraph existing, IUriNode id, IUriNode hasDimensionPredicate, DimensionParser.Dimension dimension)
    {
        var assetHasDimension = existing.GetSingleUriNode(id, hasDimensionPredicate) ?? CacheClient.NewId;
        graph.Assert(id, hasDimensionPredicate, assetHasDimension);
        GraphAssert.Integer(graph, assetHasDimension, dimension.FirstMm, Vocabulary.FirstDimensionMillimetre);
        GraphAssert.Integer(graph, assetHasDimension, dimension.SecondMm, Vocabulary.SecondDimensionMillimetre);
    }

    private static void AssertSecondDimension(IGraph graph, IGraph existing, IUriNode id, IUriNode hasDimensionPredicate, DimensionParser.Dimension dimension)
    {
        var assetHasDimension = existing.GetSingleUriNode(id, hasDimensionPredicate) ?? CacheClient.NewId;
        graph.Assert(id, hasDimensionPredicate, assetHasDimension);
        GraphAssert.Integer(graph, assetHasDimension, dimension.SecondFirstMm, Vocabulary.FirstDimensionMillimetre);
        GraphAssert.Integer(graph, assetHasDimension, dimension.SecondSecondMm, Vocabulary.SecondDimensionMillimetre);
    }
}
