select f.FILEREF from dufile f
join xmlmetadata x on x.METADATAREF = f.FMETADATAREF
where f.Code = $id
order by f.FILEREF
