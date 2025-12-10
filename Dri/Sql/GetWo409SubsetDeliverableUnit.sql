select d.DELIVERABLEUNITREF, x.XMLCLOB from xmlmetadata x
join deliverableunit p on p.METADATAREF = x.METADATAREF
join deliverableunit du on du.PARENTREF = p.DELIVERABLEUNITREF
join dufile d on d.DELIVERABLEUNITREF = du.DELIVERABLEUNITREF
where d.DELIVERABLEUNITREF = $id
limit 1
