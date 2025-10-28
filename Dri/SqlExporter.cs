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
    private readonly string duXmlSql;
    private readonly string fileXmlSql;
    private readonly string auditSql;
    private readonly string duWo409XmlSql;

    public SqlExporter(ILogger<SqlExporter> logger, IOptions<DriSettings> options)
    {
        this.logger = logger;
        settings = options.Value;

        var currentAssembly = typeof(SqlExporter).Assembly;
        var baseName = $"{typeof(SqlExporter).Namespace}.Sql";
        var embedded = new EmbeddedResource(currentAssembly, baseName);

        duXmlSql = embedded.GetSql(nameof(GetAssetDeliverableUnits));
        fileXmlSql = embedded.GetSql(nameof(GetVariationFiles));
        auditSql = embedded.GetSql(nameof(GetChanges));
        duWo409XmlSql = embedded.GetSql(nameof(GetWo409SubsetDeliverableUnits));
    }

    public IEnumerable<DriAssetDeliverableUnit> GetAssetDeliverableUnits(int offset, CancellationToken cancellationToken)
    {
        logger.GetDeliverableUnits(offset);

        static DriAssetDeliverableUnit mapping(SqliteDataReader reader) =>
            new(reader.GetString("DELIVERABLEUNITREF"), reader.GetString("CATALOGUEREFERENCE"),
                reader.GetString("XMLCLOB"), reader.GetString("files"));

        return Get(duXmlSql, offset, mapping, cancellationToken);
    }

    public IEnumerable<DriWo409SubsetDeliverableUnit> GetWo409SubsetDeliverableUnits(int offset, CancellationToken cancellationToken)
    {
        logger.GetWo409SubsetDeliverableUnits(offset);

        static DriWo409SubsetDeliverableUnit mapping(SqliteDataReader reader) =>
            new(reader.GetString("DELIVERABLEUNITREF"), reader.GetString("XMLCLOB"));

        return Get(duWo409XmlSql, offset, mapping, cancellationToken);
    }

    public IEnumerable<DriVariationFile> GetVariationFiles(int offset, CancellationToken cancellationToken)
    {
        logger.GetFiles(offset);

        static DriVariationFile mapping(SqliteDataReader reader) =>
            new(reader.GetString("FILEREF"), reader.GetString("FILELOCATION"),
                reader.GetString("NAME"), reader.GetString("MANIFESTATIONREF"), reader.GetString("XMLCLOB"));

        return Get(fileXmlSql, offset, mapping, cancellationToken);
    }

    public IEnumerable<DriChange> GetChanges(int offset, CancellationToken cancellationToken)
    {
        logger.GetChanges(offset);

        static DriChange mapping(SqliteDataReader reader) =>
            new(reader.GetValue("CHANGEREF").ToString(), reader.GetString("TABLENAME"),
                reader.GetString("PRIMARYKEYVALUE"), reader.GetDateTimeOffset(reader.GetOrdinal("DATETIME")),
                reader.GetString("USERNAME"), reader.GetString("FULLNAME"), reader.GetString("XMLDIFF"));

        return Get(auditSql, offset, mapping, cancellationToken);
    }

    private IEnumerable<T> Get<T>(string sql, int offset,
        Func<SqliteDataReader, T> mappingFunc, CancellationToken cancellationToken)
    {
        var codeParam = new SqliteParameter("$code", settings.Code);
        var limitParam = new SqliteParameter("$limit", settings.FetchPageSize);
        var offsetParam = new SqliteParameter("$offset", offset);

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
