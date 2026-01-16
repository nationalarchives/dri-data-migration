with dus as (
	select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, d.DMETADATAREF, d.SECURITYTAG from dufile d
	where d.Code = $code
    group by d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, d.DMETADATAREF, d.SECURITYTAG
),
files as (
	select d.DELIVERABLEUNITREF, d.FILEREF, d.FILELOCATION, d.NAME from dufile d
	where d.Code = $code
)
select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB, d.SECURITYTAG,
	(select concat('[',
		group_concat(distinct json_object('id', f.FILEREF, 'location', f.FILELOCATION, 'name', f.NAME )),']')
	from files f where f.DELIVERABLEUNITREF = d.DELIVERABLEUNITREF) as files
from dus d
join xmlmetadata x on x.METADATAREF = d.DMETADATAREF
order by d.DELIVERABLEUNITREF
limit $limit offset $offset
