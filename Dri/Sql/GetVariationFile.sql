select f.FILEREF, f.FILELOCATION, f.NAME, f.MANIFESTATIONREF, x.XMLCLOB from dufile f
join xmlmetadata x on x.METADATAREF = f.FMETADATAREF
where f.FILEREF = $id
