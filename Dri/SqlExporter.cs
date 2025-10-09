using Api;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Dri;

public class SqlExporter(ILogger<SqlExporter> logger, IOptions<DriSettings> options) : IDriSqlExporter
{
    private readonly DriSettings settings = options.Value;
    private readonly string duXmlSql = """
        select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB,
            concat('[',group_concat(distinct json_object('id', f.FILEREF, 'location', f.FILELOCATION, 'name', f.NAME )),']') as files
        from dufile d
        join xmlmetadata x on x.METADATAREF = d.DMETADATAREF
        join dufile f on f.DELIVERABLEUNITREF = d.DELIVERABLEUNITREF
        where d.Code = $code
        group by d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB
        order by d.rowid
        limit $limit offset $offset
        """;
    private readonly string fileXmlSql = """
        select f.FILEREF, f.FILELOCATION, f.NAME, x.XMLCLOB from dufile f
        join xmlmetadata x on x.METADATAREF = f.FMETADATAREF
        where f.Code = $code
        order by f.rowid
        limit $limit offset $offset
        """;
    private readonly string auditSql = """
        select distinct t.TABLENAME, a.CHANGEREF, a.PRIMARYKEYVALUE, a.DATETIME, a.USERNAME, a.FULLNAME, a.XMLDIFF from auditchange a
        join tableinvolved t on t.TABLEINVOLVEDREF = a.TABLEINVOLVEDREF
        join dufile d on (d.DELIVERABLEUNITREF = a.PRIMARYKEYVALUE and t.TABLENAME = 'DeliverableUnit') or
        	(d.FILEREF = a.PRIMARYKEYVALUE and t.TABLENAME = 'DigitalFile')
        where a.XMLDIFF is not null and instr(username, 'du') = 0 and d.Code = $code
        order by a.rowid
        limit $limit offset $offset
        """;
    private readonly string duWo409XmlSql = """
        select d.DELIVERABLEUNITREF, x.XMLCLOB from xmlmetadata x
        join deliverableunit p on p.METADATAREF = x.METADATAREF
        join deliverableunit du on du.PARENTREF = p.DELIVERABLEUNITREF
        join dufile d on d.DELIVERABLEUNITREF = du.DELIVERABLEUNITREF
        where d.Code = 'WO 409' and $code = 'WO 409'
        group by d.DELIVERABLEUNITREF, x.XMLCLOB
        order by d.rowid
        limit $limit offset $offset
        """;

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
                reader.GetString("NAME"), reader.GetString("XMLCLOB"));

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
