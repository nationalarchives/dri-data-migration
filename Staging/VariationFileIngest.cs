using Api;
using Microsoft.Extensions.Logging;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class VariationFileIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<VariationFileIngest> logger)
    : StagingIngest<DriVariationFile>(sparqlClient, logger, cacheClient, "VariationFileGraph")
{
    private readonly HashSet<string> predicates = [];
    private readonly DateParser dateParser = new(logger);

    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariationFile dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);

        var driId = new LiteralNode(dri.Id);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.VariationDriId, driId).FirstOrDefault()?.Subject;
        if (id is null)
        {
            logger.VariationNotFound(dri.Name, dri.Location); //TODO: sensitive information?
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationDriId, driId);
        graph.Assert(id, Vocabulary.VariationRelativeLocation, new LiteralNode($"{dri.Location}/{dri.Name}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)));
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dri.Xml));
            graph.Assert(id, Vocabulary.VariationDriXml, new LiteralNode(xmlBase64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            var proceed = await ExtractXmlData(graph, existing, id, dri.Xml, cancellationToken);
            if (!proceed)
            {
                return null;
            }
        }
        logger.RecordBuilt(dri.Id);

        return graph;
    }

    internal override void PostIngest()
    {
        Console.WriteLine("Distinct RDF predicates:");
        foreach (var predicate in predicates.OrderBy(p => p))
        {
            Console.WriteLine(predicate);
        }
    }

    private async Task<bool> ExtractXmlData(IGraph graph, IGraph existing, INode id, string xml, CancellationToken cancellationToken)
    {
        var rdf = RdfXmlLoader.GetRdf(xml, logger);
        if (rdf is null)
        {
            logger.VariationXmlMissingRdf(id.AsValuedNode().AsString());
            return false;
        }

        predicates.UnionWith(rdf.Triples.PredicateNodes.Cast<IUriNode>().Select(p => p.Uri.ToString()).ToHashSet());

        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [note] = Vocabulary.VariationNote,
            [curatedDateNote] = Vocabulary.VariationNote,
            [formerReferenceDepartment] = Vocabulary.VariationPastName,
            [physicalCondition] = Vocabulary.VariationPhysicalConditionDescription,
            [googleId] = Vocabulary.VariationReferenceGoogleId,
            [googleParentId] = Vocabulary.VariationReferenceParentGoogleId,
            [scanId] = Vocabulary.ScannerIdentifier,
            [scanOperator] = Vocabulary.ScannerOperatorIdentifier,
            [dctermsDescription] = dctermsDescription //TODO: remove after checking
        });

        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, scanLocation, CacheEntityKind.GeographicalPlace,
            Vocabulary.ScannedVariationHasScannerGeographicalPlace, Vocabulary.GeographicalPlaceName, cancellationToken);

        AddImageNodes(graph, rdf, id);

        var datedNote = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.VariationHasDatedNote).SingleOrDefault()?.Object ?? CacheClient.NewId;
        if (datedNote is not null)
        {
            var noteDate = existing.GetTriplesWithSubjectPredicate(datedNote, Vocabulary.DatedNoteHasDate).SingleOrDefault()?.Object ?? CacheClient.NewId;
            AddDatedNote(graph, rdf, id, datedNote, noteDate); //TODO: could be overengineering
        }

        return true;
    }

    private void AddImageNodes(IGraph graph, IGraph rdf, INode id)
    {
        var foundImageSplit = rdf.GetTriplesWithPredicate(imageSplit).FirstOrDefault()?.Object;
        if (foundImageSplit is ILiteralNode imageSplitNode && !string.IsNullOrWhiteSpace(imageSplitNode.Value) &&
            imageSplitNode.Value != "no")
        {
            if (imageSplitNode.Value == "yes")
            {
                graph.Assert(id, Vocabulary.ScannedVariationHasImageSplit, Vocabulary.ImageSplit);
            }
            else
            {
                logger.UnrecognizedImageSplitValue(imageSplitNode.Value);
            }
        }
        var foundImageCrop = rdf.GetTriplesWithPredicate(imageCrop).FirstOrDefault()?.Object;
        if (foundImageCrop is ILiteralNode imageCropNode && !string.IsNullOrWhiteSpace(imageCropNode.Value) &&
            imageCropNode.Value != "none")
        {
            var crop = imageCropNode.Value switch
            {
                "auto" => Vocabulary.AutoImageCrop,
                "manual" => Vocabulary.ManualImageCrop,
                _ => null
            };
            if (crop is not null)
            {
                graph.Assert(id, Vocabulary.ScannedVariationHasImageCrop, crop);
            }
            else
            {
                logger.UnrecognizedImageCropValue(imageCropNode.Value);
            }
        }
        var foundImageDeskew = rdf.GetTriplesWithPredicate(imageDeskew).FirstOrDefault()?.Object;
        if (foundImageDeskew is ILiteralNode imageDeskewNode && !string.IsNullOrWhiteSpace(imageDeskewNode.Value) &&
            imageDeskewNode.Value != "none")
        {
            var deskew = imageDeskewNode.Value switch
            {
                "auto" => Vocabulary.AutoImageDeskew,
                "manual" => Vocabulary.ManualImageDeskew,
                _ => null
            };
            if (deskew is not null)
            {
                graph.Assert(id, Vocabulary.ScannedVariationHasImageDeskew, deskew);
            }
            else
            {
                logger.UnrecognizedImageDeskewValue(imageDeskewNode.Value);
            }
        }
    }

    private void AddDatedNote(IGraph graph, IGraph rdf, INode id, INode datedNode, INode noteDate)
    {
        var foundNote = rdf.GetTriplesWithPredicate(archivistNote).FirstOrDefault()?.Object;
        if (foundNote is not null)
        {
            var info = rdf.GetTriplesWithSubjectPredicate(foundNote, archivistNoteInfo).FirstOrDefault()?.Object as ILiteralNode;
            if (info is not null && !string.IsNullOrWhiteSpace(info.Value))
            {
                graph.Assert(id, Vocabulary.VariationHasDatedNote, datedNode);
                graph.Assert(datedNode, Vocabulary.ArchivistNote, new LiteralNode(info.Value)); //TODO: review notes to check if can be better modelled
                var date = rdf.GetTriplesWithSubjectPredicate(foundNote, archivistNoteDate).FirstOrDefault()?.Object as ILiteralNode;
                if (date is not null && !string.IsNullOrWhiteSpace(date.Value))
                {
                    var ymd = dateParser.ParseDate(date.Value);
                    if (ymd.DateKind == DateParser.DateType.Date)
                    {
                        graph.Assert(datedNode, Vocabulary.DatedNoteHasDate, noteDate);
                        GraphAssert.YearMonthDay(graph, noteDate, ymd.Year, ymd.Month, ymd.Day);
                    }
                }
            }
        }
    }



    private static readonly IUriNode note = new UriNode(new($"{Vocabulary.TnaNamespace}note"));
    private static readonly IUriNode curatedDateNote = new UriNode(new($"{Vocabulary.TnaNamespace}curatedDateNote"));
    private static readonly IUriNode formerReferenceDepartment = new UriNode(new($"{Vocabulary.TnaNamespace}formerReferenceDepartment"));
    private static readonly IUriNode physicalCondition = new UriNode(new($"{Vocabulary.TnaNamespace}physicalCondition"));
    private static readonly IUriNode googleId = new UriNode(new($"{Vocabulary.TnaNamespace}googleId"));
    private static readonly IUriNode googleParentId = new UriNode(new($"{Vocabulary.TnaNamespace}googleParentId"));
    private static readonly IUriNode archivistNote = new UriNode(new($"{Vocabulary.TnaNamespace}archivistNote"));
    private static readonly IUriNode archivistNoteInfo = new UriNode(new($"{Vocabulary.TnaNamespace}archivistNoteInfo"));
    private static readonly IUriNode archivistNoteDate = new UriNode(new($"{Vocabulary.TnaNamespace}archivistNoteDate"));
    private static readonly IUriNode scanOperator = new UriNode(new($"{Vocabulary.TnaNamespace}scanOperator"));
    private static readonly IUriNode scanId = new UriNode(new($"{Vocabulary.TnaNamespace}scanId"));
    private static readonly IUriNode scanLocation = new UriNode(new($"{Vocabulary.TnaNamespace}scanLocation"));
    private static readonly IUriNode imageSplit = new UriNode(new($"{Vocabulary.TnaNamespace}imageSplit"));
    private static readonly IUriNode imageCrop = new UriNode(new($"{Vocabulary.TnaNamespace}imageCrop"));
    private static readonly IUriNode imageDeskew = new UriNode(new($"{Vocabulary.TnaNamespace}imageDeskew"));

    private static readonly IUriNode dctermsDescription = new UriNode(new("http://purl.org/dc/terms/description")); //TODO: remove after checking data
}
