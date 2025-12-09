with files as (
	select d.DELIVERABLEUNITREF, d.FILEREF, d.FILELOCATION, d.NAME from dufile d
	where d.DELIVERABLEUNITREF = $id
)
select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, x.XMLCLOB,
	concat('[',
		group_concat(distinct json_object('id', f.FILEREF, 'location', f.FILELOCATION, 'name', f.NAME )),
	']') as files
from dufile d
join xmlmetadata x on x.METADATAREF = d.DMETADATAREF
join files f on f.DELIVERABLEUNITREF = d.DELIVERABLEUNITREF
where d.DELIVERABLEUNITREF = $id
group by d.DELIVERABLEUNITREF

