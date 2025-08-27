using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class SensitivityReviewIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<SensitivityReviewIngest> logger)
    : StagingIngest<DriSensitivityReview>(sparqlClient, logger, "SensitivityReviewGraph")
{
    private Dictionary<string, IUriNode> legislations;
    private Dictionary<string, IUriNode> accessConditions;
    private Dictionary<string, IUriNode> groundsForRetention;

    internal override async Task<Graph?> BuildAsync(IGraph existing, DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        await PreloadAsync(cancellationToken);

        var id = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewDriId).FirstOrDefault()?.Subject ?? BaseIngest.NewId;
        var restriction = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewHasSensitivityReviewRestriction).FirstOrDefault()?.Object ?? BaseIngest.NewId;
        var retentionRestriction = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction).FirstOrDefault()?.Object ?? BaseIngest.NewId;

        var graph = new Graph();

        var proceed = await AddSensitivityReview(graph, id, dri, cancellationToken);
        if (!proceed)
        {
            return null;
        }

        proceed = AddRestriction(graph, id, restriction, dri);
        if (!proceed)
        {
            return null;
        }

        proceed = await AddTargetAssociation(graph, id, dri, cancellationToken);
        if (!proceed)
        {
            return null;
        }

        proceed = await AddRetentionRestriction(graph, id, restriction, retentionRestriction, dri, cancellationToken);
        if (!proceed)
        {
            return null;
        }
        logger.RecordBuilt(dri.Id);

        return graph;
    }

    private async Task PreloadAsync(CancellationToken cancellationToken)
    {
        accessConditions = await cacheClient.AccessConditions(cancellationToken);
        legislations = await cacheClient.Legislations(cancellationToken);
        groundsForRetention = await cacheClient.GroundsForRetention(cancellationToken);

        if (accessConditions is null || !accessConditions.Any())
        {
            logger.MissingAccessConditions();
            throw new MigrationException();
        }
        if (legislations is null || !legislations.Any())
        {
            logger.MissingLegislations();
            throw new MigrationException();
        }
        if (groundsForRetention is null || !groundsForRetention.Any())
        {
            logger.MissingLegislations();
            throw new MigrationException();
        }
    }

    private async Task<bool> AddSensitivityReview(IGraph graph, INode id, DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        graph.Assert(id, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.Id));
        if (dri.Date.HasValue)
        {
            graph.Assert(id, Vocabulary.SensitivityReviewDate, new DateNode(dri.Date.Value));
        }
        if (!string.IsNullOrEmpty(dri.SensitiveName))
        {
            graph.Assert(id, Vocabulary.SensitivityReviewSensitiveName, new LiteralNode(dri.SensitiveName));
        }
        if (!string.IsNullOrEmpty(dri.SensitiveDescription))
        {
            graph.Assert(id, Vocabulary.SensitivityReviewSensitiveDescription, new LiteralNode(dri.SensitiveDescription));
        }

        if (dri.AccessCondition is not null)
        {
            var acCode = BaseIngest.GetUriFragment(dri.AccessCondition);
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
            var past = await cacheClient.CacheFetchOrNew(CacheEntityKind.SensititvityReview, dri.PreviousId.ToString(), Vocabulary.SensitivityReviewDriId, cancellationToken);
            graph.Assert(id, Vocabulary.SensitivityReviewHasPastSensitivityReview, past);
        }

        return true;
    }

    private bool AddRestriction(IGraph graph, INode id, INode restriction, DriSensitivityReview dri)
    {
        var existing = graph.Triples.Count;

        if (dri.ReviewDate.HasValue)
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate, new DateNode(dri.ReviewDate.Value));
        }
        if (dri.RestrictionStartDate.HasValue)
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate, new DateNode(dri.RestrictionStartDate.Value));
        }
        if (dri.RestrictionDuration.HasValue)
        {
            var yearType = new string[] { "D", "U" };
            if (yearType.Contains(BaseIngest.GetUriFragment(dri.AccessCondition)))
            {
                graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionEndYear, new LiteralNode(dri.RestrictionDuration.Value.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeYear)));
            }
            else
            {
                graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDuration, new LiteralNode($"P{dri.RestrictionDuration.Value}Y", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
            }
        }
        if (!string.IsNullOrEmpty(dri.RestrictionDescription))
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDescription, new LiteralNode(dri.RestrictionDescription));
        }
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

    private async Task<bool> AddTargetAssociation(IGraph graph, INode id, DriSensitivityReview dri, CancellationToken cancellationToken)
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
            var variation = await cacheClient.CacheFetch(CacheEntityKind.Variation, dri.TargetId.ToString(), cancellationToken);
            if (variation is null)
            {
                logger.VariationNotFound(dri.TargetId.ToString());
                return false;
            }
            graph.Assert(id, Vocabulary.SensitivityReviewHasVariation, variation);
        }

        return true;
    }

    private async Task<bool> AddRetentionRestriction(IGraph graph, INode id, INode restriction, INode retentionRestriction,
        DriSensitivityReview dri, CancellationToken cancellationToken)
    {
        var existing = graph.Triples.Count;

        if (dri.InstrumentNumber.HasValue)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(dri.InstrumentNumber.Value));
        }
        if (dri.InstrumentSignedDate.HasValue)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate, new DateNode(dri.InstrumentSignedDate.Value));
        }
        if (dri.RestrictionReviewDate.HasValue)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate, new DateNode(dri.RestrictionReviewDate.Value));
        }
        if (dri.GroundForRetention is not null)
        {
            var gCode = BaseIngest.GetUriFragment(dri.GroundForRetention);
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
}
