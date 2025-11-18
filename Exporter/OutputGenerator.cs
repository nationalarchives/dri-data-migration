using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        if (settings.ExportScope is not ExportScopeType.XML)
        {
            await GenerateRecordAsync(cancellationToken);
        }
        if (settings.ExportScope is not ExportScopeType.JSON)
        {
            await GenerateXmlAsync(cancellationToken);
        }
        logger.ExportFinished();
    }

    private async Task GenerateRecordAsync(CancellationToken cancellationToken)
    {
        List<RecordOutput> records;
        int offset = settings.RestartFromOffset;
        do
        {
            records = (await recordRetrieval.GetRecordAsync(offset, cancellationToken)).ToList();
            logger.ExportingRecords(records.Count);
            offset += settings.FetchPageSize;
            Serialize(records);
            logger.RecordsExported();
        } while (records.Count > 0);
    }

    private async Task GenerateXmlAsync(CancellationToken cancellationToken)
    {
        List<XmlWrapper> xmls;
        int offset = settings.RestartFromOffset;
        do
        {
            xmls = (await recordRetrieval.GetXmlAsync(offset, cancellationToken)).ToList();
            logger.ExportingXmls(xmls.Count);
            offset += settings.FetchPageSize;
            Serialize(xmls);
            logger.XmlsExported();
        } while (xmls.Count > 0);
    }

    private void Serialize(List<RecordOutput> records)
    {
        foreach (var record in records)
        {
            using (logger.BeginScope(("RecordId", record.Reference)))
            {
                logger.SerializingRecord();
                try
                {
                    var fileName = FileName(record.Reference, "json");
                    var json = JsonSerializer.Serialize(record, serializerOptions);
                    File.WriteAllText(fileName, json);
                }
                catch (Exception e)
                {
                    logger.UnableSerialize(record.Reference);
                    logger.SerializationProblem(e);
                }
            }
        }
    }

    private void Serialize(List<XmlWrapper> xmls)
    {
        foreach (var xml in xmls)
        {
            using (logger.BeginScope(("RecordId", xml.Reference)))
            {
                logger.SerializingXml();
                try
                {
                    var fileName = FileName(xml.Reference, "xml");
                    File.WriteAllText(fileName, xml.Xml);
                }
                catch (Exception e)
                {
                    logger.UnableSerialize(xml.Reference);
                    logger.SerializationProblem(e);
                }
            }
        }
    }

    private string FileName(string reference, string extension) =>
        $"{exportPath}\\{string.Join('-', reference.Split(invalidCharacters))}.{extension}";
}
