﻿using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class AccessConditionIngest(ISparqlClient sparqlClient, ILogger<AccessConditionIngest> logger) :
    StagingIngest<DriAccessCondition>(sparqlClient, logger, "AccessConditionGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriAccessCondition dri, CancellationToken cancellationToken)
    {
        var code = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.AccessConditionCode, code) ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AccessConditionCode, code);
        GraphAssert.Text(graph, id, dri.Name, Vocabulary.AccessConditionName);

        return Task.FromResult((Graph?)graph);
    }
}
