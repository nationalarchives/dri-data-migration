select d.DELIVERABLEUNITREF, x.XMLCLOB from xmlmetadata x
join deliverableunit p on p.METADATAREF = x.METADATAREF
join deliverableunit du on du.PARENTREF = p.DELIVERABLEUNITREF
join dufile d on d.DELIVERABLEUNITREF = du.DELIVERABLEUNITREF
where d.Code = 'WO 409' and $code = 'WO 409'
group by d.DELIVERABLEUNITREF, x.XMLCLOB
order by d.rowid
limit $limit offset $offset