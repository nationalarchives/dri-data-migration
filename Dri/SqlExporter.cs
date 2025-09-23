using Api;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Dri;

public class SqlExporter(ILogger<SqlExporter> logger, IOptions<DriSettings> options) : IDriSqlExporter
{
    /*
    Additional indices created:
    
    create index xmlmetadata_ix on xmlmetadata (METADATAREF)
    create index deliverableunit_ix on deliverableunit (DELETED)
    create index deliverableunit_ix2 on deliverableunit (DELETED, IsWO409)
    create index deliverableunit_ix3 on deliverableunit (DELIVERABLEUNITREF)
    create index deliverableunitmanifestation_ix on deliverableunitmanifestation (DELETED, ACTIVE, MANIFESTATIONREF)
    create index deliverableunitmanifestation_ix2 on deliverableunitmanifestation (DELETED, ACTIVE)
    create index manifestationfile_ix on manifestationfile (FILEREF)
    create index manifestationfile_ix2 on manifestationfile (MANIFESTATIONREF)
    create index digitalfile_ix on digitalfile (FILEREF, DELETED)
    create index auditchange_ix on auditchange(PRIMARYKEYVALUE)
    create index tableinvolved_ix on tableinvolved (TABLEINVOLVEDREF);

    Additional column added:

    alter table deliverableunit add column IsWO409 integer default 0
    update deliverableunit set IsWO409 = 1 where substr(DESCRIPTION,1,10)='WO/16/409/'
     */
    private readonly DriSettings settings = options.Value;
    private readonly string duXmlSql = """
        select distinct d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB from digitalfile f
        join manifestationfile m on m.FILEREF = f.FILEREF
        join deliverableunitmanifestation dm on dm.MANIFESTATIONREF = m.MANIFESTATIONREF
        join deliverableunit d on d.DELIVERABLEUNITREF = dm.DELIVERABLEUNITREF
        join xmlmetadata x on x.METADATAREF = d.METADATAREF
        where f.DELETED = 'F' and f.EXTANT = 'T' and f.DIRECTORY = 'F' and
            dm.DELETED = 'F' and dm.ACTIVE='T' and d.DELETED = 'F' and
            (d.TOPLEVELREF = (select TOPLEVELREF from deliverableunit where DELETED = 'F' and DESCRIPTION = $code) or
                ($code = 'WO 409' and dm.ORIGINALITY = 'T' and
                d.TOPLEVELREF in (select TOPLEVELREF from deliverableunit where DELETED = 'F' and IsWO409 = 1)))
        order by d.rowid
        limit $limit offset $offset
        """;
    private readonly string fileXmlSql = """
        select f.FILEREF, f.FILELOCATION, f.NAME, x.XMLCLOB from digitalfile f
        join manifestationfile m on m.FILEREF = f.FILEREF
        join deliverableunitmanifestation dm on dm.MANIFESTATIONREF = m.MANIFESTATIONREF
        join deliverableunit d on d.DELIVERABLEUNITREF = dm.DELIVERABLEUNITREF
        join xmlmetadata x on x.METADATAREF = f.METADATAREF
        where f.DELETED = 'F' and f.EXTANT = 'T' and f.DIRECTORY = 'F' and
            dm.DELETED = 'F' and dm.ACTIVE='T' and d.DELETED = 'F' and
            (d.TOPLEVELREF = (select TOPLEVELREF from deliverableunit where DELETED = 'F' and DESCRIPTION = $code) or
                ($code = 'WO 409' and dm.ORIGINALITY = 'T' and
                d.TOPLEVELREF in (select TOPLEVELREF from deliverableunit where DELETED = 'F' and IsWO409 = 1)))
        order by f.rowid
        limit $limit offset $offset
        """;
    private readonly string auditSql = """
        select distinct c.CHANGEREF, c.TABLENAME, c.PRIMARYKEYVALUE, c.DATETIME, c.USERNAME, c.FULLNAME, c.XMLDIFF from digitalfile f
        join manifestationfile m on m.FILEREF = f.FILEREF
        join deliverableunitmanifestation dm on dm.MANIFESTATIONREF = m.MANIFESTATIONREF
        join deliverableunit d on d.DELIVERABLEUNITREF = dm.DELIVERABLEUNITREF
        join(
        	select t.TABLENAME, a.CHANGEREF, a.PRIMARYKEYVALUE, a.DATETIME, a.USERNAME, a.FULLNAME, a.XMLDIFF from auditchange a
        	join tableinvolved t on t.TABLEINVOLVEDREF = a.TABLEINVOLVEDREF
        	where a.XMLDIFF is not null
        ) c on (c.PRIMARYKEYVALUE = d.DELIVERABLEUNITREF and c.TABLENAME = 'DeliverableUnit') or
        	(c.PRIMARYKEYVALUE = f.FILEREF and c.TABLENAME = 'DigitalFile')
        where f.DELETED = 'F' and f.EXTANT = 'T' and f.DIRECTORY = 'F' and
            dm.DELETED = 'F' and dm.ACTIVE='T' and d.DELETED = 'F' and
            (d.TOPLEVELREF = (select TOPLEVELREF from deliverableunit where DELETED = 'F' and DESCRIPTION = $code) or
                ($code = 'WO 409' and dm.ORIGINALITY = 'T' and
                d.TOPLEVELREF in (select TOPLEVELREF from deliverableunit where DELETED = 'F' and IsWO409 = 1)))
        order by f.rowid
        limit $limit offset $offset
        """;

    public IEnumerable<DriAssetDeliverableUnit> GetAssetDeliverableUnits(int offset, CancellationToken cancellationToken)
    {
        logger.GetDeliverableUnits(offset);
        var codeParam = new SqliteParameter("$code", settings.Code);
        var limitParam = new SqliteParameter("$limit", settings.FetchPageSize);
        var offsetParam = new SqliteParameter("$offset", offset);

        using var connection = new SqliteConnection(settings.SqlConnectionString);
        cancellationToken.Register(() => SQLitePCL.raw.sqlite3_interrupt(connection.Handle));
        connection.Open();
        using var command = new SqliteCommand(duXmlSql, connection);
        command.Parameters.AddRange([codeParam, limitParam, offsetParam]);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new DriAssetDeliverableUnit(
                reader.GetString("DELIVERABLEUNITREF"), reader.GetString("CATALOGUEREFERENCE"), reader.GetString("XMLCLOB"));
        }
    }

    public IEnumerable<DriVariationFile> GetVariationFiles(int offset, CancellationToken cancellationToken)
    {
        logger.GetFiles(offset);
        var codeParam = new SqliteParameter("$code", settings.Code);
        var limitParam = new SqliteParameter("$limit", settings.FetchPageSize);
        var offsetParam = new SqliteParameter("$offset", offset);

        using var connection = new SqliteConnection(settings.SqlConnectionString);
        cancellationToken.Register(() => SQLitePCL.raw.sqlite3_interrupt(connection.Handle));
        connection.Open();
        using var command = new SqliteCommand(fileXmlSql, connection);
        command.Parameters.AddRange([codeParam, limitParam, offsetParam]);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new DriVariationFile(reader.GetString("FILEREF"),
                reader.GetString("FILELOCATION"), reader.GetString("NAME"),
                reader.GetString("XMLCLOB"));
        }
    }

    public IEnumerable<DriChange> GetChanges(int offset, CancellationToken cancellationToken)
    {
        logger.GetChanges(offset);
        var codeParam = new SqliteParameter("$code", settings.Code);
        var limitParam = new SqliteParameter("$limit", settings.FetchPageSize);
        var offsetParam = new SqliteParameter("$offset", offset);

        using var connection = new SqliteConnection(settings.SqlConnectionString);
        cancellationToken.Register(() => SQLitePCL.raw.sqlite3_interrupt(connection.Handle));
        connection.Open();
        using var command = new SqliteCommand(auditSql, connection);
        command.Parameters.AddRange([codeParam, limitParam, offsetParam]);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new DriChange(reader.GetValue("CHANGEREF").ToString(),
                reader.GetString("TABLENAME"), reader.GetString("PRIMARYKEYVALUE"),
                reader.GetDateTimeOffset(reader.GetOrdinal("DATETIME")), reader.GetString("USERNAME"),
                reader.GetString("FULLNAME"), reader.GetString("XMLDIFF"));
        }
    }
}
