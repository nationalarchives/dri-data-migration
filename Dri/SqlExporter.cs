using Api;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;

namespace Dri;

public class SqlExporter(IOptions<DriSettings> options) : IDriSqlExporter
{
    /* Additional indices created:
    create index xmlmetadata_ix on xmlmetadata (METADATAREF)
    create index deliverableunit_ix on deliverableunit (TOPLEVELREF, DELETED)
    create index deliverableunitmanifestation_ix on deliverableunitmanifestation (DELIVERABLEUNITREF, MANIFESTATIONREF, DELETED, ACTIVE)
    create index manifestationfile_ix on manifestationfile (MANIFESTATIONREF)
    create index digitalfile_ix on digitalfile (FILEREF, DELETED)
     */

    private readonly DriSettings settings = options.Value;
    private readonly string duXmlSql = """
        select distinct d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB from deliverableunit d
        join deliverableunitmanifestation dm on d.DELIVERABLEUNITREF = dm.DELIVERABLEUNITREF
        join manifestationfile m on dm.MANIFESTATIONREF = m.MANIFESTATIONREF
        join digitalfile f on m.FILEREF = f.FILEREF
        join xmlmetadata x on x.METADATAREF = d.METADATAREF
        where d.DELETED = 'F' and dm.DELETED = 'F' and dm.ACTIVE = 'T' and d.TOPLEVELREF = (select TOPLEVELREF from deliverableunit where DELETED = 'F' and DESCRIPTION = $code)
        order by d.DELIVERABLEUNITREF
        limit $limit offset $offset
        """;
    /*"""
    with records(DELIVERABLEUNITREF, CATALOGUEREFERENCE, XMLCLOB, TOPLEVELREF) as (
        select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB, d.TOPLEVELREF from deliverableunit d
        join xmlmetadata x on x.METADATAREF = d.METADATAREF
        where d.DELETED = 'F' and d.DESCRIPTION  = $code
        union
        select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB, d.TOPLEVELREF from deliverableunit d
        join deliverableunitmanifestation dm on dm.DELIVERABLEUNITREF = d.DELIVERABLEUNITREF
        join manifestationfile m on m.MANIFESTATIONREF = dm.MANIFESTATIONREF
        join xmlmetadata x on x.METADATAREF = d.METADATAREF
        join records r on r.TOPLEVELREF = d.TOPLEVELREF
        where d.DELETED = 'F' and dm.DELETED='F' and dm.ACTIVE='T'
    )
    select DELIVERABLEUNITREF, CATALOGUEREFERENCE, XMLCLOB from records
    order by DELIVERABLEUNITREF
    limit $limit offset $offset
    """;*/
    private readonly string fileXmlSql = """
        select f.FILEREF, f.FILELOCATION, f.NAME, x.XMLCLOB from digitalfile f
        join manifestationfile m on m.FILEREF = f.FILEREF
        join deliverableunitmanifestation dm on dm.MANIFESTATIONREF = m.MANIFESTATIONREF
        join deliverableunit d on d.DELIVERABLEUNITREF = dm.DELIVERABLEUNITREF
        join xmlmetadata x on x.METADATAREF = f.METADATAREF
        where f.DELETED = 'F' and dm.DELETED = 'F' and dm.ACTIVE='T' and d.DELETED = 'F' and
            d.TOPLEVELREF = (select TOPLEVELREF from deliverableunit where DELETED = 'F' and DESCRIPTION  = $code)
        order by f.FILEREF
        limit $limit offset $offset
        """;

    public IEnumerable<DriAssetDeliverableUnit> GetAssetDeliverableUnits(int offset)
    {
        var codeParam = new SqliteParameter("$code", settings.Code);
        var limitParam = new SqliteParameter("$limit", settings.FetchPageSize);
        var offsetParam = new SqliteParameter("$offset", offset);

        using var connection = new SqliteConnection(settings.SqlConnectionString);
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

    public IEnumerable<DriVariationFile> GetVariationFiles(int offset)
    {
        var codeParam = new SqliteParameter("$code", settings.Code);
        var limitParam = new SqliteParameter("$limit", settings.FetchPageSize);
        var offsetParam = new SqliteParameter("$offset", offset);

        using var connection = new SqliteConnection(settings.SqlConnectionString);
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
}
