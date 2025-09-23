using Api;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

namespace Dri.Tests;

[TestClass]
public sealed class SqlExporterTest
{
    private const string Series = "Series 1";
    private const string SqlSchema = $"""
            create table digitalfile
                (FILEREF TEXT, METADATAREF TEXT, FILELOCATION TEXT, NAME TEXT, DELETED TEXT DEFAULT 'F', EXTANT TEXT DEFAULT 'T', DIRECTORY TEXT DEFAULT 'F');
            create table manifestationfile
                (FILEREF TEXT, MANIFESTATIONREF TEXT);
            create table deliverableunitmanifestation
                (MANIFESTATIONREF TEXT, DELIVERABLEUNITREF TEXT, DELETED TEXT DEFAULT 'F', ACTIVE TEXT DEFAULT 'T', ORIGINALITY TEXT DEFAULT 'T' );
            create table deliverableunit
                (DELIVERABLEUNITREF TEXT, CATALOGUEREFERENCE TEXT, DESCRIPTION TEXT, TOPLEVELREF TEXT, METADATAREF TEXT, DELETED TEXT DEFAULT 'F', IsWO409 INTEGER);
            create table xmlmetadata
                (METADATAREF TEXT, XMLCLOB TEXT);
            create table auditchange
                (CHANGEREF TEXT, PRIMARYKEYVALUE TEXT, TABLEINVOLVEDREF TEXT, DATETIME TEXT, USERNAME TEXT, FULLNAME TEXT, XMLDIFF TEXT);
            create table tableinvolved
                (TABLEINVOLVEDREF TEXT, TABLENAME TEXT);
        """;
    private SqlExporter exporter;
    private IOptions<DriSettings> options;
    private FakeLogger<SqlExporter> logger;

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

    [TestMethod("Reads asset deliverable units")]
    public void FetchesAssetDeliverableUnits()
    {
        var sqliteInMemory = "Data Source=file:memdb-asset?mode=memory&cache=shared";
        options.Value.SqlConnectionString = sqliteInMemory;
        var expected = new DriAssetDeliverableUnit("Asset1", "Asset", "<xml/>");
        PopulateAsset(expected, sqliteInMemory);

        var dris = exporter.GetAssetDeliverableUnits(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads variation files")]
    public void FetchesVariationFiles()
    {
        var sqliteInMemory = "Data Source=file:memdb-variation?mode=memory&cache=shared";
        options.Value.SqlConnectionString = sqliteInMemory;
        var expected = new DriVariationFile("Variation1", "Location", "Variation name", "<xml/>");
        PopulateVariation(expected, sqliteInMemory);

        var dris = exporter.GetVariationFiles(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads changes")]
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
        var fileRef = "File reference asset";
        var manifestationRef = "Manifestation reference asset";
        var metadataRef = "Metadata reference asset";
        var topLevelRef = "Top level reference asset";
        var data = $"""
            insert into digitalfile(FILEREF) values('{fileRef}');
            insert into manifestationfile(MANIFESTATIONREF, FILEREF) values('{manifestationRef}', '{fileRef}');
            insert into deliverableunitmanifestation(MANIFESTATIONREF, DELIVERABLEUNITREF) values('{manifestationRef}', '{dri.Id}');
            insert into deliverableunit(DELIVERABLEUNITREF, METADATAREF, DESCRIPTION, TOPLEVELREF, CATALOGUEREFERENCE) values('{dri.Id}', '{metadataRef}', '{Series}', '{topLevelRef}', '{dri.Reference}');
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
        var manifestationRef = "Manifestation reference variation";
        var metadataRef = "Metadata reference variation";
        var topLevelRef = "Top level reference variation";

        var data = $"""
            insert into digitalfile(FILEREF, METADATAREF, FILELOCATION, NAME) values('{dri.Id}', '{metadataRef}', '{dri.Location}', '{dri.Name}');
            insert into manifestationfile(MANIFESTATIONREF, FILEREF) values('{manifestationRef}', '{dri.Id}');
            insert into deliverableunitmanifestation(MANIFESTATIONREF, DELIVERABLEUNITREF) values('{manifestationRef}', '{dri.Id}');
            insert into deliverableunit(DELIVERABLEUNITREF, DESCRIPTION, TOPLEVELREF) values('{dri.Id}', '{Series}', '{topLevelRef}');
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
        var manifestationRef = "Manifestation reference asset";
        var topLevelRef = "Top level reference variation";
        var tableInvolvedRef = "Table involved ref";

        var data = $"""
            insert into digitalfile(FILEREF) values('{fileRef}');
            insert into manifestationfile(MANIFESTATIONREF, FILEREF) values('{manifestationRef}', '{fileRef}');
            insert into deliverableunitmanifestation(MANIFESTATIONREF, DELIVERABLEUNITREF) values('{manifestationRef}', '{dri.Reference}');
            insert into deliverableunit(DELIVERABLEUNITREF, DESCRIPTION, TOPLEVELREF) values('{dri.Reference}', '{Series}', '{topLevelRef}');
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
}