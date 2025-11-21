using Api;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Dri.Tests;

[TestClass]
public sealed class SqlExporterTest
{
    private const string Series = "Series 1";
    private const string SqlSchema = $"""
            create table dufile
                (DELIVERABLEUNITREF TEXT, CATALOGUEREFERENCE TEXT, DMETADATAREF TEXT, MANIFESTATIONREF TEXT, FILEREF TEXT, FMETADATAREF TEXT, FILELOCATION TEXT, NAME TEXT, Code TEXT);
            create table xmlmetadata
                (METADATAREF TEXT, XMLCLOB TEXT);
            create table auditchange
                (CHANGEREF TEXT, PRIMARYKEYVALUE TEXT, TABLEINVOLVEDREF TEXT, DATETIME TEXT, USERNAME TEXT, FULLNAME TEXT, XMLDIFF TEXT);
            create table tableinvolved
                (TABLEINVOLVEDREF TEXT, TABLENAME TEXT);
        """;
#pragma warning disable CS8618
    private SqlExporter exporter;
    private IOptions<DriSettings> options;
    private FakeLogger<SqlExporter> logger;
#pragma warning restore CS8618

    [TestInitialize]
    public void TestInitialize()
    {
        logger = new FakeLogger<SqlExporter>();
        options = Options.Create<DriSettings>(new()
        {
            Code = Series,
            FetchPageSize = 1
        });
        exporter = new SqlExporter(logger, options);
    }

    [TestMethod(DisplayName = "Reads asset deliverable units")]
    public void FetchesAssetDeliverableUnits()
    {
        var sqliteInMemory = "Data Source=file:memdb-asset?mode=memory&cache=shared";
        options.Value.SqlConnectionString = sqliteInMemory;
        var expected = new DriAssetDeliverableUnit("Asset1", "Asset", "<xml/>", "[{\"id\":\"Variation1\",\"location\":\"Location\",\"name\":\"Variation name\"}]");
        PopulateAsset(expected, sqliteInMemory);

        var dris = exporter.GetAssetDeliverableUnits(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod(DisplayName = "Reads variation files")]
    public void FetchesVariationFiles()
    {
        var sqliteInMemory = "Data Source=file:memdb-variation?mode=memory&cache=shared";
        options.Value.SqlConnectionString = sqliteInMemory;
        var expected = new DriVariationFile("Variation1", "Location", "Variation name", "Manifestation1", "<xml/>");
        PopulateVariation(expected, sqliteInMemory);

        var dris = exporter.GetVariationFiles(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod(DisplayName = "Reads changes")]
    public void FetchesChanges()
    {
        var sqliteInMemory = "Data Source=file:memdb-change?mode=memory&cache=shared";
        options.Value.SqlConnectionString = sqliteInMemory;
        var expected = new DriChange("Change1", "DeliverableUnit", "Asset1", DateTimeOffset.UtcNow, "Username1", "First Second", "Change diff");
        PopulateChange(expected, sqliteInMemory);

        var dris = exporter.GetChanges(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    private static void PopulateAsset(DriAssetDeliverableUnit dri, string sqliteConnectionString)
    {
        var file = JsonSerializer.Deserialize<List<FileJsonTest>>(
            dri.FilesJson, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            ?.SingleOrDefault();
        var metadataRef = "Metadata reference asset";
        var data = $"""
            insert into dufile(DELIVERABLEUNITREF, DMETADATAREF, CATALOGUEREFERENCE, Code, FILEREF, FILELOCATION, NAME)
                values('{dri.Id}', '{metadataRef}', '{dri.Reference}', '{Series}', '{file.Id}', '{file.Location}','{file.Name}');
            insert into xmlmetadata(METADATAREF, XMLCLOB) values('{metadataRef}', '{dri.Xml}');
        """;

        using var connection = new SqliteConnection(sqliteConnectionString);
        connection.Open();
        using var commandSchema = new SqliteCommand(SqlSchema, connection);
        commandSchema.ExecuteNonQuery();
        using var commandData = new SqliteCommand(data, connection);
        commandData.ExecuteNonQuery();
    }

    private static void PopulateVariation(DriVariationFile dri, string sqliteConnectionString)
    {
        var metadataRef = "Metadata reference variation";
        var data = $"""
            insert into dufile(FILEREF, FMETADATAREF, FILELOCATION, NAME, Code, MANIFESTATIONREF) values('{dri.Id}', '{metadataRef}', '{dri.Location}', '{dri.Name}', '{Series}', '{dri.ManifestationId}');
            insert into xmlmetadata(METADATAREF, XMLCLOB) values('{metadataRef}', '{dri.Xml}');
        """;

        using var connection = new SqliteConnection(sqliteConnectionString);
        connection.Open();
        using var commandSchema = new SqliteCommand(SqlSchema, connection);
        commandSchema.ExecuteNonQuery();
        using var commandData = new SqliteCommand(data, connection);
        commandData.ExecuteNonQuery();
    }

    private static void PopulateChange(DriChange dri, string sqliteConnectionString)
    {
        var fileRef = "File reference asset";
        var tableInvolvedRef = "Table involved ref";
        var data = $"""
            insert into dufile(DELIVERABLEUNITREF, FILEREF, Code) values('{dri.Reference}', '{fileRef}', '{Series}');
            insert into auditchange(CHANGEREF, PRIMARYKEYVALUE, TABLEINVOLVEDREF, DATETIME, USERNAME, FULLNAME, XMLDIFF)
                values('{dri.Id}', '{dri.Reference}', '{tableInvolvedRef}', '{dri.Timestamp:O}', '{dri.UserName}', '{dri.FullName}', '{dri.Diff}');
            insert into tableinvolved(TABLEINVOLVEDREF, TABLENAME) values('{tableInvolvedRef}', '{dri.Table}');
        """;

        using var connection = new SqliteConnection(sqliteConnectionString);
        connection.Open();
        using var commandSchema = new SqliteCommand(SqlSchema, connection);
        commandSchema.ExecuteNonQuery();
        using var commandData = new SqliteCommand(data, connection);
        commandData.ExecuteNonQuery();
    }

    private record FileJsonTest(string Id, string Location, string Name);
}