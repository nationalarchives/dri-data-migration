using Api;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Dri;

public class SqlExporter : IDriSqlExporter
{
    private readonly ILogger<SqlExporter> logger;
    private readonly DriSettings settings;
    private readonly EmbeddedResource embedded;

    public SqlExporter(ILogger<SqlExporter> logger, IOptions<DriSettings> options)
    {
        this.logger = logger;
        settings = options.Value;

        var currentAssembly = typeof(SqlExporter).Assembly;
        var baseName = $"{typeof(SqlExporter).Namespace}.Sql";
        embedded = new(currentAssembly, baseName);
    }

    public IEnumerable<DriAssetDeliverableUnit> GetAssetDeliverableUnits(int offset, CancellationToken cancellationToken) =>
        Get(EtlStageType.AssetDeliverableUnit, offset, MapAssetDeliverableUnit, cancellationToken);

    public IEnumerable<DriWo409SubsetDeliverableUnit> GetWo409SubsetDeliverableUnits(int offset, CancellationToken cancellationToken) =>
        Get(EtlStageType.Wo409SubsetDeliverableUnit, offset, MapWo409SubsetDeliverableUnit, cancellationToken);

    public IEnumerable<DriVariationFile> GetVariationFiles(int offset, CancellationToken cancellationToken) =>
        Get(EtlStageType.VariationFile, offset, MapVariationFile, cancellationToken);

    public IEnumerable<DriChange> GetChanges(int offset, CancellationToken cancellationToken) =>
        Get(EtlStageType.Change, offset, MapChange, cancellationToken);

    private static DriAssetDeliverableUnit MapAssetDeliverableUnit(SqliteDataReader reader) =>
        new(reader.GetString("DELIVERABLEUNITREF"), reader.GetString("CATALOGUEREFERENCE"),
            reader.GetString("XMLCLOB"), reader.GetString("files"));

    private static DriWo409SubsetDeliverableUnit MapWo409SubsetDeliverableUnit(SqliteDataReader reader) =>
            new(reader.GetString("DELIVERABLEUNITREF"), reader.GetString("XMLCLOB"));

    private static DriVariationFile MapVariationFile(SqliteDataReader reader) =>
            new(reader.GetString("FILEREF"), reader.GetString("FILELOCATION"),
                reader.GetString("NAME"), reader.GetString("MANIFESTATIONREF"), reader.GetString("XMLCLOB"));

    private static DriChange MapChange(SqliteDataReader reader) =>
            new(reader.GetValue("CHANGEREF").ToString(), reader.GetString("TABLENAME"),
                reader.GetString("PRIMARYKEYVALUE"), reader.GetDateTimeOffset(reader.GetOrdinal("DATETIME")),
                reader.GetString("USERNAME"), reader.GetString("FULLNAME"), reader.GetString("XMLDIFF"));

    private IEnumerable<T> Get<T>(EtlStageType stageType, int offset,
        Func<SqliteDataReader, T> mappingFunc, CancellationToken cancellationToken)
    {
        logger.FetchingRecordsOffset(stageType, offset);
        var codeParam = new SqliteParameter("$code", settings.Code);
        var limitParam = new SqliteParameter("$limit", settings.FetchPageSize);
        var offsetParam = new SqliteParameter("$offset", offset);
        var sql = embedded.GetSql($"Get{stageType}");

        using var connection = new SqliteConnection(settings.SqlConnectionString);
        connection.Open();
        RegisterCancellation(connection, cancellationToken);
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddRange([codeParam, limitParam, offsetParam]);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return mappingFunc(reader);
        }
    }

    private static void RegisterCancellation(SqliteConnection connection, CancellationToken cancellationToken) =>
        cancellationToken.Register(() =>
        {
            if (connection.State is not ConnectionState.Closed)
            {
                SQLitePCL.raw.sqlite3_interrupt(connection.Handle);
            }
        });
}
