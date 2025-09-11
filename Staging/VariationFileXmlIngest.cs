using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Staging;

public class VariationFileXmlIngest(ILogger logger, ICacheClient cacheClient)
{
    public readonly HashSet<string> Predicates = [];
    private readonly GraphAssert assert = new(logger, cacheClient);
    private readonly DateParser dateParser = new(logger);

    public async Task<bool> ExtractXmlData(IGraph graph, IGraph existing, INode id, string xml, CancellationToken cancellationToken)
    {
        var rdf = RdfXmlLoader.GetRdf(xml, logger);
        if (rdf is null)
        {
            logger.VariationXmlMissingRdf(id.AsValuedNode().AsString());
            return false;
        }

        Predicates.UnionWith(rdf.Triples.PredicateNodes.Cast<IUriNode>().Select(p => p.Uri.ToString()).ToHashSet());

        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.Note] = Vocabulary.VariationNote,
            [IngestVocabulary.CuratedDateNote] = Vocabulary.VariationNote,
            [IngestVocabulary.PhysicalCondition] = Vocabulary.VariationPhysicalConditionDescription,
            [IngestVocabulary.GoogleId] = Vocabulary.VariationReferenceGoogleId,
            [IngestVocabulary.GoogleParentId] = Vocabulary.VariationReferenceParentGoogleId,
            [IngestVocabulary.ScanId] = Vocabulary.ScannerIdentifier,
            [IngestVocabulary.ScanOperator] = Vocabulary.ScannerOperatorIdentifier,
            [IngestVocabulary.DctermsDescription] = IngestVocabulary.DctermsDescription //TODO: remove after checking
        });

        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, IngestVocabulary.ScanLocation, CacheEntityKind.GeographicalPlace,
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
        var foundImageSplit = rdf.GetTriplesWithPredicate(IngestVocabulary.ImageSplit).FirstOrDefault()?.Object;
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
        var foundImageCrop = rdf.GetTriplesWithPredicate(IngestVocabulary.ImageCrop).FirstOrDefault()?.Object;
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
        var foundImageDeskew = rdf.GetTriplesWithPredicate(IngestVocabulary.ImageDeskew).FirstOrDefault()?.Object;
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
        var foundNote = rdf.GetTriplesWithPredicate(IngestVocabulary.ArchivistNote).FirstOrDefault()?.Object;
        if (foundNote is not null)
        {
            var info = rdf.GetTriplesWithSubjectPredicate(foundNote, IngestVocabulary.ArchivistNoteInfo).FirstOrDefault()?.Object as ILiteralNode;
            if (info is not null && !string.IsNullOrWhiteSpace(info.Value))
            {
                graph.Assert(id, Vocabulary.VariationHasDatedNote, datedNode);
                graph.Assert(datedNode, Vocabulary.ArchivistNote, new LiteralNode(info.Value)); //TODO: review notes to check if can be better modelled
                var date = rdf.GetTriplesWithSubjectPredicate(foundNote, IngestVocabulary.ArchivistNoteDate).FirstOrDefault()?.Object as ILiteralNode;
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
}
