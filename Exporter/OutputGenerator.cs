using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using VDS.RDF;

namespace Exporter;

public class OutputGenerator(ILogger<OutputGenerator> logger, IOptions<ExportSettings> settings,
    IRecordRetrieval recordRetrieval) : IOutputGenerator
{
    private readonly ExportSettings settings = settings.Value;
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    private const string exportPath = "export";
    private readonly char[] invalidCharacters = Path.GetInvalidFileNameChars();

    public async Task GenerateOutputAsync(CancellationToken cancellationToken)
    {
        var path = Directory.CreateDirectory(exportPath);
        logger.ExportPath(path.FullName);
        logger.ExportStarted(settings.ExportScope, settings.Code);
        var ids = (await recordRetrieval.GetListAsync(settings.Code, cancellationToken))?.ToList();
        if (ids is not null)
        {
            logger.RecordListFound(ids.Count);
            await GenerateRecordAsync(ids, cancellationToken);
        }
        logger.ExportFinished();
    }

    private async Task GenerateRecordAsync(List<IUriNode> ids, CancellationToken cancellationToken)
    {
        var i= 0;
        foreach (var id in ids.Skip(settings.RestartFromOffset))
        {
            i++;
            using (logger.BeginScope(("RecordId", id.Uri)))
            {
                logger.GeneratingRecord();
                if (settings.ExportScope is not ExportScopeType.XML)
                {
                    var records = await recordRetrieval.GetRecordAsync(id, cancellationToken);
                    if (!records.Any())
                    {
                        logger.UnableFindRecord(id.Uri);
                    }
                    Serialize(records);
                }
                if (settings.ExportScope is not ExportScopeType.JSON)
                {
                    var xmls = await recordRetrieval.GetXmlAsync(id, cancellationToken);
                    Serialize(xmls);
                }
            }
            if (i % 500 == 0)
            {
                logger.ExportRecordCount(i);
            }
        }
        if (i % 500 != 0)
        {
            logger.ExportRecordCount(i);
        }
    }

    private void Serialize(IEnumerable<RecordOutput> records)
    {
        foreach (var record in records)
        {
            try
            {
                var fileName = FileName(record.Reference, "json");
                var json = JsonSerializer.Serialize(record, serializerOptions);
                if (File.Exists(fileName))
                {
                    logger.ExistingFileRecord(fileName);
                }
                File.WriteAllText(fileName, json);
            }
            catch (Exception e)
            {
                logger.UnableSerialize(record.Reference);
                logger.SerializationProblem(e);
            }
        }
    }

    private void Serialize(IEnumerable<XmlWrapper> xmls)
    {
        foreach (var xml in xmls)
        {
            try
            {
                var fileName = FileName(xml.Reference, "xml");
                if (File.Exists(fileName))
                {
                    logger.ExistingFileRecord(fileName);
                }
                File.WriteAllText(fileName, xml.Xml);
            }
            catch (Exception e)
            {
                logger.UnableSerialize(xml.Reference);
                logger.SerializationProblem(e);
            }
        }
    }

    private string FileName(string reference, string extension) =>
        $"{exportPath}\\{string.Join('-', reference.Split(invalidCharacters))}.{extension}";
}
