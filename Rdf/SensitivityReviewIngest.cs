using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Rdf;

public class SensitivityReviewIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger logger)
    : StagingIngest<DriSensitivityReview>(cache, sparqlClient, logger, "SensitivityReviewGraph")
{
    private Dictionary<string, IUriNode>? accessConditions = null;
    private Dictionary<string, IUriNode>? legislations = null;
    private Dictionary<string, IUriNode>? grounds = null;

    internal override async Task<Graph> BuildAsync(IGraph existing, DriSensitivityReview dri)
    {
        accessConditions ??= await sparqlClient.GetDictionaryAsync(embedded.GetSparql("GetAccessConditions"));
        legislations ??= await sparqlClient.GetDictionaryAsync(embedded.GetSparql("GetLegislations"));
        grounds ??= await sparqlClient.GetDictionaryAsync(embedded.GetSparql("GetGroundsForRetention"));

        var id = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewDriId).FirstOrDefault()?.Subject ?? NewId;
        var restriction = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewHasSensitivityReviewRestriction).FirstOrDefault()?.Object ?? NewId;
        var retentionRestriction = existing.GetTriplesWithPredicate(Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction).FirstOrDefault()?.Object ?? NewId;

        var graph = new Graph();

        await AddSensitivityReview(graph, id, dri);

        AddRestriction(graph, id, restriction, dri);

        await AddTargetAssociation(graph, id, dri);

        await AddRetentionRestriction(graph, id, restriction, retentionRestriction, dri);

        return graph;
    }

    private async Task AddSensitivityReview(IGraph graph, INode id, DriSensitivityReview dri)
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
        //TODO: handle null
        graph.Assert(id, Vocabulary.SensitivityReviewHasAccessCondition, accessConditions![GetUriFragment(dri.AccessCondition)]);
        
        if (dri.PreviousId is not null)
        {
            var past = await CacheFetchOrNew(CacheEntityKind.SensititvityReview, dri.PreviousId.ToString());
            graph.Assert(id, Vocabulary.SensitivityReviewHasPastSensitivityReview, past);
            graph.Assert(past, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.PreviousId.ToString()));
        }
    }

    private void AddRestriction(IGraph graph, INode id, INode restriction, DriSensitivityReview dri)
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
            if (yearType.Contains(GetUriFragment(dri.AccessCondition)))
            {
                graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionEndYear, new LongNode(dri.RestrictionDuration.Value));
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
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislations![legislation.ToString()]);
        }

        if (existing < graph.Triples.Count)
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
        }
    }

    private async Task AddTargetAssociation(IGraph graph, INode id, DriSensitivityReview dri)
    {
        if (dri.TargetType.Fragment == "#DeliverableUnit")
        {
            var asset = await CacheFetch(CacheEntityKind.Asset, dri.TargetReference);
            if (asset is not null)
            {
                graph.Assert(id, Vocabulary.SensitivityReviewHasAsset, asset);
            }
            else
            {
                var subset = await CacheFetch(CacheEntityKind.Subset, dri.TargetReference);
                //TODO: handle null
                graph.Assert(id, Vocabulary.SensitivityReviewHasSubset, subset);
            }
        }
        else
        {
            var variation = await CacheFetch(CacheEntityKind.Variation, dri.TargetId.ToString());
            //TODO: handle null
            graph.Assert(id, Vocabulary.SensitivityReviewHasVariation, variation);
        }
    }

    private async Task AddRetentionRestriction(IGraph graph, INode id, INode restriction, INode retentionRestriction,
        DriSensitivityReview dri)
    {
        var existing = graph.Triples.Count;

        if (dri.InstrumentNumber.HasValue)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(dri.InstrumentNumber.Value));
        }
        if (dri.InstrumentSignedDate.HasValue)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentSignedDate, new DateNode(dri.InstrumentSignedDate.Value));
        }
        if (dri.RestrictionReviewDate.HasValue)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate, new DateNode(dri.RestrictionReviewDate.Value));
        }
        if (dri.GroundForRetention is not null)
        {
            //TODO: handle null
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, grounds[GetUriFragment(dri.GroundForRetention)]);
        }

        if (existing < graph.Triples.Count)
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction, retentionRestriction);
            var asset = graph.GetTriplesWithSubjectPredicate(id, Vocabulary.SensitivityReviewHasAsset).SingleOrDefault()?.Object as IUriNode;
            if (asset is not null)
            {
                var retention = await CacheFetch(CacheEntityKind.Retention, asset.Uri.ToString());
                //TODO: handle null
                graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasRetention, retention);
                graph.Assert(asset, Vocabulary.AssetHasRetention, retention);
            }
            var subset = graph.GetTriplesWithSubjectPredicate(id, Vocabulary.SensitivityReviewHasSubset).SingleOrDefault()?.Object as IUriNode;
            if (subset is not null)
            {
                var retention = await CacheFetch(CacheEntityKind.Retention, subset.Uri.ToString());
                //TODO: handle null
                graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasRetention, retention);
                graph.Assert(subset, Vocabulary.SubsetHasRetention, retention);
            }
        }
    }
}
