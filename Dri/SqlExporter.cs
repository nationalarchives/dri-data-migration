using Api;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Dri;

public class SqlExporter : IDriSqlExporter
{
    private readonly DriSettings settings;
    private readonly EmbeddedResource embedded;
    private readonly string duXmlSql;
    private readonly string fileXmlSql;
    private readonly string auditSql;
    private readonly string duWo409XmlSql;

    public SqlExporter(IOptions<DriSettings> options)
    {
        settings = options.Value;

        var currentAssembly = typeof(SqlExporter).Assembly;
        var baseName = $"{typeof(SqlExporter).Namespace}.Sql";
        embedded = new EmbeddedResource(currentAssembly, baseName);

        duXmlSql = embedded.GetSql($"Get{EtlStageType.AssetDeliverableUnit}");
        fileXmlSql = embedded.GetSql($"Get{EtlStageType.VariationFile}");
        auditSql = embedded.GetSql($"Get{EtlStageType.Change}");
        duWo409XmlSql = embedded.GetSql($"Get{EtlStageType.Wo409SubsetDeliverableUnit}");
    }

    public IEnumerable<string> GetList(EtlStageType etlStageType, CancellationToken cancellationToken)
    {
        var sql = embedded.GetSql($"List{etlStageType}");
        var idParam = new SqliteParameter("$id", settings.Code);

        using var connection = new SqliteConnection(settings.SqlConnectionString);
        connection.Open();
        RegisterCancellation(connection, cancellationToken);
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(idParam);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return reader.GetValue(0).ToString();
        }
    }

    public DriAssetDeliverableUnit GetAssetDeliverableUnit(string id, CancellationToken cancellationToken)
    {
        static DriAssetDeliverableUnit mapping(SqliteDataReader reader) =>
            new(reader.GetString("DELIVERABLEUNITREF"), reader.GetString("CATALOGUEREFERENCE"),
                reader.GetString("XMLCLOB"), reader.GetString("files"));

        return Get(duXmlSql, id, mapping, cancellationToken);
    }

    public DriWo409SubsetDeliverableUnit GetWo409SubsetDeliverableUnit(string id, CancellationToken cancellationToken)
    {
        static DriWo409SubsetDeliverableUnit mapping(SqliteDataReader reader) =>
            new(reader.GetString("DELIVERABLEUNITREF"), reader.GetString("XMLCLOB"));

        return Get(duWo409XmlSql, id, mapping, cancellationToken);
    }

    public DriVariationFile GetVariationFile(string id, CancellationToken cancellationToken)
    {
        static DriVariationFile mapping(SqliteDataReader reader) =>
            new(reader.GetString("FILEREF"), reader.GetString("FILELOCATION"),
                reader.GetString("NAME"), reader.GetString("MANIFESTATIONREF"), reader.GetString("XMLCLOB"));

        return Get(fileXmlSql, id, mapping, cancellationToken);
    }

    public DriChange GetChange(string id, CancellationToken cancellationToken)
    {
        static DriChange mapping(SqliteDataReader reader) =>
            new(reader.GetValue("CHANGEREF").ToString(), reader.GetString("TABLENAME"),
                reader.GetString("PRIMARYKEYVALUE"), reader.GetDateTimeOffset(reader.GetOrdinal("DATETIME")),
                reader.GetString("USERNAME"), reader.GetString("FULLNAME"), reader.GetString("XMLDIFF"));

        return Get(auditSql, id, mapping, cancellationToken);
    }

    private T Get<T>(string sql, string id,
        Func<SqliteDataReader, T> mappingFunc, CancellationToken cancellationToken)
    {
        var idParam = new SqliteParameter("$id", id);

        using var connection = new SqliteConnection(settings.SqlConnectionString);
        connection.Open();
        RegisterCancellation(connection, cancellationToken);
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(idParam);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return mappingFunc(reader);
        }
        throw new MigrationException($"Unable to fetch {id}");
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
