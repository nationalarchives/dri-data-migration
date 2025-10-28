using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class SensitivityReviewIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<SensitivityReviewIngest> logger) :
    StagingIngest<DriSensitivityReview>(sparqlClient, logger, "SensitivityReviewGraph")
{
    private Dictionary<string, IUriNode> legislations;
    private Dictionary<string, IUriNode> accessConditions;
    private Dictionary<string, IUriNode> groundsForRetention;

    internal override async Task<Graph?> BuildAsync(IGraph existing, DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        await PreloadAsync(cancellationToken);

        var id = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewDriId).FirstOrDefault()?.Subject ?? CacheClient.NewId;
        var restriction = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewHasSensitivityReviewRestriction).FirstOrDefault()?.Object ?? CacheClient.NewId;
        var retentionRestriction = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction).FirstOrDefault()?.Object ?? CacheClient.NewId;

        var graph = new Graph();

        var proceed = await AddSensitivityReviewAsync(graph, id, dri, cancellationToken);
        if (!proceed)
        {
            return null;
        }

        proceed = AddRestriction(graph, id, restriction, dri);
        if (!proceed)
        {
            return null;
        }

        proceed = await AddTargetAssociationAsync(graph, id, dri, cancellationToken);
        if (!proceed)
        {
            return null;
        }

        proceed = await AddRetentionRestrictionAsync(graph, id, restriction, retentionRestriction, dri, cancellationToken);
        if (!proceed)
        {
            return null;
        }
        await AddChangeAsync(graph, existing, id, dri, cancellationToken);

        return graph;
    }

    private async Task PreloadAsync(CancellationToken cancellationToken)
    {
        accessConditions = await cacheClient.AccessConditions(cancellationToken);
        legislations = await cacheClient.Legislations(cancellationToken);
        groundsForRetention = await cacheClient.GroundsForRetention(cancellationToken);

        if (accessConditions is null || accessConditions.Count == 0)
        {
            logger.MissingAccessConditions();
            throw new MigrationException();
        }
        if (legislations is null || legislations.Count == 0)
        {
            logger.MissingLegislations();
            throw new MigrationException();
        }
        if (groundsForRetention is null || groundsForRetention.Count == 0)
        {
            logger.MissingLegislations();
            throw new MigrationException();
        }
    }

    private async Task<bool> AddSensitivityReviewAsync(IGraph graph, INode id, DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        graph.Assert(id, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.Id));
        GraphAssert.Date(graph, id, dri.Date, Vocabulary.SensitivityReviewDate);
        GraphAssert.Text(graph, id, new Dictionary<IUriNode, string?>()
        {
            [Vocabulary.SensitivityReviewSensitiveName] = dri.SensitiveName,
            [Vocabulary.SensitivityReviewSensitiveDescription] = dri.SensitiveDescription
        });
        if (dri.AccessCondition is not null)
        {
            var acCode = GetUriFragment(dri.AccessCondition);
            if (acCode is null)
            {
                logger.UnableParseAccessConditionUri(dri.AccessCondition);
                return false;
            }
            if (!accessConditions!.TryGetValue(acCode, out var ac))
            {
                logger.AccessConditionNotFound(acCode);
                return false;
            }
            graph.Assert(id, Vocabulary.SensitivityReviewHasAccessCondition, ac);
        }
        if (dri.PreviousId is not null)
        {
            var past = await cacheClient.CacheFetchOrNew(CacheEntityKind.SensititvityReview, dri.PreviousId, Vocabulary.SensitivityReviewDriId, cancellationToken);
            graph.Assert(id, Vocabulary.SensitivityReviewHasPastSensitivityReview, past);
        }

        return true;
    }

    private bool AddRestriction(Graph graph, INode id, INode restriction, DriSensitivityReview dri)
    {
        var existing = graph.Triples.Count;

        GraphAssert.Date(graph, restriction, new Dictionary<IUriNode, DateTimeOffset?>()
        {
            [Vocabulary.SensitivityReviewRestrictionReviewDate] = dri.ReviewDate,
            [Vocabulary.SensitivityReviewRestrictionCalculationStartDate] = dri.RestrictionStartDate
        });
        if (dri.RestrictionDuration.HasValue)
        {
            var yearType = new string[] { "D", "U" };
            if (yearType.Contains(GetUriFragment(dri.AccessCondition)))
            {
                graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionEndYear, new LiteralNode(dri.RestrictionDuration.Value.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeYear)));
            }
            else
            {
                graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDuration, new LiteralNode($"P{dri.RestrictionDuration.Value}Y", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
            }
        }
        GraphAssert.Text(graph, restriction, dri.RestrictionDescription, Vocabulary.SensitivityReviewRestrictionDescription);
        foreach (var legislation in dri.Legislations)
        {
            if (!legislations!.TryGetValue(legislation.ToString(), out var legislationNode))
            {
                logger.LegislationNotFound(legislation.ToString());
                return false;
            }
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislationNode);
        }

        if (existing < graph.Triples.Count)
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
        }

        return true;
    }

    private async Task<bool> AddTargetAssociationAsync(IGraph graph, INode id, DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        if (dri.TargetType.Fragment == "#DeliverableUnit")
        {
            var asset = await cacheClient.CacheFetch(CacheEntityKind.Asset, dri.TargetReference, cancellationToken);
            if (asset is not null)
            {
                graph.Assert(id, Vocabulary.SensitivityReviewHasAsset, asset);
            }
            else
            {
                var subset = await cacheClient.CacheFetch(CacheEntityKind.Subset, dri.TargetReference, cancellationToken);
                if (subset is null)
                {
                    logger.SubsetNotFound(dri.TargetReference);
                    return false;
                }
                graph.Assert(id, Vocabulary.SensitivityReviewHasSubset, subset);
            }
        }
        else
        {
            var variation = await cacheClient.CacheFetch(CacheEntityKind.Variation, dri.TargetId, cancellationToken);
            if (variation is null)
            {
                logger.AssociatedVariationNotFound(dri.TargetReference); //TODO: sensitive name?
                return false;
            }
            graph.Assert(id, Vocabulary.SensitivityReviewHasVariation, variation);
        }

        return true;
    }

    private async Task<bool> AddRetentionRestrictionAsync(Graph graph, INode id, INode restriction, INode retentionRestriction,
        DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        var existing = graph.Triples.Count;

        if (dri.InstrumentNumber.HasValue)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(dri.InstrumentNumber.Value));
        }
        GraphAssert.Date(graph, retentionRestriction, new Dictionary<IUriNode, DateTimeOffset?>()
        {
            [Vocabulary.RetentionInstrumentSignatureDate] = dri.InstrumentSignedDate,
            [Vocabulary.RetentionRestrictionReviewDate] = dri.RestrictionReviewDate
        });
        if (dri.GroundForRetention is not null)
        {
            var gCode = GetUriFragment(dri.GroundForRetention);
            if (gCode is null)
            {
                logger.UnableParseGroundForRetentionUri(dri.GroundForRetention);
                return false;
            }
            if (!groundsForRetention!.TryGetValue(gCode, out var ground))
            {
                logger.GroundForRetentionNotFound(gCode);
                return false;
            }
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, ground);
        }

        if (existing < graph.Triples.Count)
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction, retentionRestriction);
            var asset = graph.GetTriplesWithSubjectPredicate(id, Vocabulary.SensitivityReviewHasAsset).SingleOrDefault()?.Object as IUriNode;
            if (asset is not null)
            {
                var retention = await cacheClient.CacheFetch(CacheEntityKind.Retention, asset.Uri.ToString(), cancellationToken);
                if (retention is null)
                {
                    logger.RetentionNotFound(asset.Uri);
                    return false;
                }
                graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasRetention, retention);
                graph.Assert(asset, Vocabulary.AssetHasRetention, retention);
            }
            var subset = graph.GetTriplesWithSubjectPredicate(id, Vocabulary.SensitivityReviewHasSubset).SingleOrDefault()?.Object as IUriNode;
            if (subset is not null)
            {
                var retention = await cacheClient.CacheFetch(CacheEntityKind.Retention, subset.Uri.ToString(), cancellationToken);
                if (retention is null)
                {
                    logger.RetentionNotFound(subset.Uri);
                    return false;
                }
                graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasRetention, retention);
                graph.Assert(subset, Vocabulary.SubsetHasRetention, retention);
            }
        }
        return true;
    }

    private async Task AddChangeAsync(IGraph graph, IGraph existing, INode id,
        DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        if (dri.ChangeId is null)
        {
            return;
        }
        var change = existing.GetTriplesWithPredicate(Vocabulary.ChangeDriId)
            .SingleOrDefault(t => t.Object.AsValuedNode().AsString() == dri.ChangeId)?.Subject ?? CacheClient.NewId;
        graph.Assert(id, Vocabulary.SensitivityReviewHasChange, change);
        GraphAssert.Text(graph, change, dri.ChangeId, Vocabulary.ChangeDriId);
        GraphAssert.Text(graph, change, dri.ChangeDescription, Vocabulary.ChangeDescription);
        if (dri.ChangeTimestamp is not null)
        {
            graph.Assert(change, Vocabulary.ChangeDateTime, new DateTimeNode(dri.ChangeTimestamp.Value));
        }
        if (dri.ChangeOperatorId is not null)
        {
            var operatorId = await cacheClient.CacheFetchOrNew(CacheEntityKind.Operator,
                dri.ChangeOperatorId, Vocabulary.OperatorIdentifier, cancellationToken);
            graph.Assert(change, Vocabulary.ChangeHasOperator, operatorId);
            GraphAssert.Text(graph, operatorId, dri.ChangeOperatorName, Vocabulary.OperatorName);
        }
    }
}
