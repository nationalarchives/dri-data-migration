select d.DELIVERABLEUNITREF from dufile d
join xmlmetadata x on x.METADATAREF = d.DMETADATAREF
where d.Code = $id
group by d.DELIVERABLEUNITREF
order by d.DELIVERABLEUNITREF
