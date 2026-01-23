select f.FILEREF, f.FILELOCATION, f.NAME, f.MANIFESTATIONREF, x.XMLCLOB, f.Checksums, f.FileSize from dufile f
join xmlmetadata x on x.METADATAREF = f.FMETADATAREF
where f.Code = $code
order by f.rowid
limit $limit offset $offset