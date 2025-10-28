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
        logger.ExportStarted(settings.Code);
        List<RecordOutput> records;
        int offset = settings.RestartFromOffset;
        do
        {
            records = (await recordRetrieval.GetAsync(offset, cancellationToken)).ToList();
            logger.ExportingRecords(records.Count);
            offset += settings.FetchPageSize;
            Serialize(records);
            logger.RecordsExported();

        } while (records.Any() && records.Count == settings.FetchPageSize);
        logger.ExportFinished();
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
                    var json = JsonSerializer.Serialize(record, serializerOptions);
                    var validReference = string.Join('-', record.Reference.Split(invalidCharacters));
                    var fileName = $"{exportPath}\\{validReference}.json";
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
}
