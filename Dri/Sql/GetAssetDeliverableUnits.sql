select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB,
    concat('[',group_concat(distinct json_object('id', f.FILEREF, 'location', f.FILELOCATION, 'name', f.NAME )),']') as files
from dufile d
join xmlmetadata x on x.METADATAREF = d.DMETADATAREF
join dufile f on f.DELIVERABLEUNITREF = d.DELIVERABLEUNITREF
where d.Code = $code
group by d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB
order by d.rowid
limit $limit offset $offset