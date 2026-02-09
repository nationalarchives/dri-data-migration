using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using VDS.RDF;

namespace Exporter;

public class OutputGenerator(ILogger<OutputGenerator> logger, IOptions<ExportSettings> exportSettings,
    IRecordRetrieval recordRetrieval) : IOutputGenerator
{
    private readonly ExportSettings settings = exportSettings.Value;

    private readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private readonly char[] invalidCharacters = Path.GetInvalidFileNameChars();

    public async Task GenerateOutputAsync(CancellationToken cancellationToken)
    {
        var exportPath = $"export\\{ToValidName(exportSettings.Value.Code)}";
        var path = Directory.CreateDirectory(exportPath);
        logger.ExportPath(path.FullName);
        logger.ExportStarted(settings.ExportScope, settings.Code);
        var ids = (await recordRetrieval.GetListAsync(settings.Code, cancellationToken))?.ToList();
        if (ids is not null)
        {
            logger.RecordListFound(ids.Count);
            await GenerateRecordAsync(exportPath, ids, cancellationToken);
        }
        logger.ExportFinished();
    }

    private async Task GenerateRecordAsync(string exportPath, List<IUriNode> ids, CancellationToken cancellationToken)
    {
        var i = 0;
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
                    Serialize(exportPath, records);
                }
                if (settings.ExportScope is not ExportScopeType.JSON)
                {
                    var xmls = await recordRetrieval.GetXmlAsync(id, cancellationToken);
                    Serialize(exportPath, xmls);
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

    private void Serialize(string exportPath, IEnumerable<RecordOutput> records)
    {
        foreach (var record in records)
        {
            try
            {
                var fileName = FileName(exportPath, record.Reference, "json");
                var json = RecordToJson(record, fileName);
                File.WriteAllText(fileName, json);
            }
            catch (Exception e)
            {
                logger.UnableSerialize(record.Reference);
                logger.SerializationProblem(e);
            }
        }
    }

    private void Serialize(string exportPath, IEnumerable<XmlWrapper> xmls)
    {
        foreach (var xml in xmls)
        {
            try
            {
                var fileName = FileName(exportPath, xml.Reference, "xml");
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

    private string FileName(string exportPath, string reference, string extension) =>
        $"{exportPath}\\{string.Join('-', reference.Split(invalidCharacters))}.{extension}";

    private string RecordToJson(RecordOutput record, string fileName)
    {
        var medicals = new string[] {"WO 409/27/102/1059", "WO 409/27/4/678",
            "WO 409/27/51/738", "WO 409/27/70/1074", "WO 409/27/93/662",
            "WO 409/27/93/663", "WO 409/27/93/664", "WO 409/27/93/665" };
        if (File.Exists(fileName))
        {
            logger.ExistingFileRecord(fileName);

            if (medicals.Contains(record.Reference))
            {
                return GenerateWO409MedicalRecord(record, fileName);
            }
        }

        return JsonSerializer.Serialize(record, serializerOptions);
    }

    private string GenerateWO409MedicalRecord(RecordOutput record, string fileName)
    {
        var json = File.ReadAllText(fileName);
        var existing = JsonSerializer.Deserialize<RecordOutput>(json, serializerOptions);
        if (existing is null)
        {
            logger.UnableDeserialize(fileName);
            return JsonSerializer.Serialize(record, serializerOptions);
        }
        if (record.DigitalFileCount > 0)
        {
            existing.DigitalFileCount += record.DigitalFileCount;
            var digitalFiles = (existing.DigitalFiles ?? []).ToList();
            digitalFiles.AddRange(record.DigitalFiles!);
            existing.DigitalFiles = digitalFiles;
        }
        if (record.AuditTrail?.Any() == true)
        {
            var auditTrail = (existing.AuditTrail ?? []).ToList();
            auditTrail.AddRange(record.AuditTrail!);
            existing.AuditTrail = auditTrail;
        }

        return JsonSerializer.Serialize(existing, serializerOptions);
    }

    private string ToValidName(string name) => string.Join('-', name.Split(invalidCharacters));
}
