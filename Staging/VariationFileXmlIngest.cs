using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Staging;

internal class VariationFileXmlIngest(ILogger logger, ICacheClient cacheClient)
{
    private readonly DateParser dateParser = new(logger);
    private readonly RdfXmlLoader rdfXmlLoader = new(logger);

    public async Task ExtractXmlData(IGraph graph, IGraph existing, IUriNode id, string xml, CancellationToken cancellationToken)
    {
        var rdf = rdfXmlLoader.GetRdf(xml);
        if (rdf is null)
        {
            logger.VariationXmlMissingRdf(id.AsValuedNode().AsString());
            return;
        }

        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.Note] = Vocabulary.VariationNote,
            [IngestVocabulary.Comment] = Vocabulary.VariationNote,
            [IngestVocabulary.PhysicalCondition] = Vocabulary.VariationPhysicalConditionDescription,
            [IngestVocabulary.GoogleId] = Vocabulary.VariationReferenceGoogleId,
            [IngestVocabulary.GoogleParentId] = Vocabulary.VariationReferenceParentGoogleId,
            [IngestVocabulary.ScanId] = Vocabulary.ScannerIdentifier,
            [IngestVocabulary.ScanOperator] = Vocabulary.ScannerOperatorIdentifier,
            [IngestVocabulary.CuratedTitle] = Vocabulary.VariationAlternativeName,
            [IngestVocabulary.Description] = IngestVocabulary.Description //TODO: remove after checking
        });
        GraphAssert.Integer(logger, graph, id, rdf, IngestVocabulary.Ordinal, Vocabulary.VariationSequence);

        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, id, rdf,
            IngestVocabulary.ScanLocation, CacheEntityKind.GeographicalPlace,
            Vocabulary.ScannedVariationHasScannerGeographicalPlace, Vocabulary.GeographicalPlaceName, cancellationToken);

        AddImageNodes(graph, rdf, id);

        AddDatedNote(graph, existing, rdf, id); //TODO: could be overengineering
    }

    private void AddImageNodes(IGraph graph, IGraph rdf, INode id)
    {
        var foundImageSplit = rdf.GetTriplesWithPredicate(IngestVocabulary.ImageSplit).FirstOrDefault()?.Object;
        if (foundImageSplit is ILiteralNode imageSplitNode && !string.IsNullOrWhiteSpace(imageSplitNode.Value) &&
            imageSplitNode.Value != "no")
        {
            var split = imageSplitNode.Value switch
            {
                "yes" => Vocabulary.ImageSplit,
                "composite" => Vocabulary.CompositeImageSplit,
                _ => null
            };
            if (split is not null)
            {
                graph.Assert(id, Vocabulary.ScannedVariationHasImageSplit, split);
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
            imageDeskewNode.Value != "none" && imageDeskewNode.Value != "no")
        {
            var deskew = imageDeskewNode.Value switch
            {
                "auto" => Vocabulary.AutoImageDeskew,
                "manual" => Vocabulary.ManualImageDeskew,
                "yes" => Vocabulary.ImageDeskew,
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

    private void AddDatedNote(IGraph graph, IGraph existing, IGraph rdf, IUriNode id)
    {
        var datedNoteNodes = existing.GetUriNodes(id, Vocabulary.VariationHasDatedNote);
        var foundNote = rdf.GetTriplesWithPredicate(IngestVocabulary.ArchivistNote).SingleOrDefault()?.Object;
        if (foundNote is not null)
        {
            var info = rdf.GetSingleLiteral(foundNote, IngestVocabulary.ArchivistNoteInfo);
            if (info is not null && !string.IsNullOrWhiteSpace(info.Value))
            {
                var datedNote = datedNoteNodes.SingleOrDefault(dn => existing.GetSingleUriNode(dn, Vocabulary.DatedNoteHasDate) is not null) ?? CacheClient.NewId;
                var noteDate = existing.GetSingleUriNode(datedNote, Vocabulary.DatedNoteHasDate) ?? CacheClient.NewId;

                graph.Assert(id, Vocabulary.VariationHasDatedNote, datedNote);
                GraphAssert.Text(graph, datedNote, info.Value, Vocabulary.ArchivistNote); //TODO: review notes to check if can be better modelled
                var date = rdf.GetSingleLiteral(foundNote, IngestVocabulary.ArchivistNoteDate);
                if (date is not null && !string.IsNullOrWhiteSpace(date.Value))
                {
                    var ymd = dateParser.ParseDate(date.Value);
                    if (ymd.DateKind == DateParser.DateType.Date)
                    {
                        graph.Assert(datedNote, Vocabulary.DatedNoteHasDate, noteDate);
                    }
                    GraphAssert.YearMonthDay(graph, noteDate, ymd.Year, ymd.Month, ymd.Day, date.Value);
                }
            }
        }
    }

}
