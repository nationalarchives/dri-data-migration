create index deliverableunit_ix on deliverableunit (DELETED);
create index deliverableunitmanifestation_ix on deliverableunitmanifestation (DELETED, ACTIVE);
create index manifestationfile_ix on manifestationfile (MANIFESTATIONREF);
create index digitalfile_ix on digitalfile (FILEREF, DELETED);
create index xmlmetadata_ix on xmlmetadata (METADATAREF);
create index digitalfilefixityinfo_ix on digitalfilefixityinfo (FILEREF);
create index fixityalgorithm_ix on fixityalgorithm (FIXITYALGORITHMREF);

create temp table temptop(TOPLEVELREF TEXT, DESCRIPTION TEXT);

insert into temptop(TOPLEVELREF, DESCRIPTION)
select TOPLEVELREF,
    case 
	    when substr(DESCRIPTION,1,10) = 'WO/16/409/' then 'WO 409'
        when substr(CATALOGUEREFERENCE,1,5) = 'WO/95' then 'WO 95'
	    else DESCRIPTION
    end as Code from deliverableunit
where DELIVERABLEUNITREF = TOPLEVELREF and DELETED = 'F';

create index temptop_ix on temptop(TOPLEVELREF);

create temp table tempdu(DELIVERABLEUNITREF TEXT, CATALOGUEREFERENCE TEXT, DMETADATAREF TEXT, Code TEXT, SECURITYTAG TEXT);

insert into tempdu(DELIVERABLEUNITREF, CATALOGUEREFERENCE, DMETADATAREF, Code, SECURITYTAG)
select d.DELIVERABLEUNITREF, d.CATALOGUEREFERENCE, d.METADATAREF, t.DESCRIPTION, d.SECURITYTAG from deliverableunit d
join temptop t on t.TOPLEVELREF = d.TOPLEVELREF
where d.DELETED = 'F';

create index tempdu_ix on tempdu(DELIVERABLEUNITREF);

create table dufile(DELIVERABLEUNITREF TEXT, FILEREF TEXT, CATALOGUEREFERENCE TEXT, DMETADATAREF TEXT, MANIFESTATIONREF TEXT, Code TEXT, 
	SECURITYTAG TEXT, FMETADATAREF TEXT, FILELOCATION TEXT, NAME TEXT, Checksums TEXT, FileSize INTEGER);

insert into dufile(DELIVERABLEUNITREF, FILEREF, CATALOGUEREFERENCE, DMETADATAREF, Code, SECURITYTAG, MANIFESTATIONREF, FMETADATAREF, FILELOCATION, NAME, Checksums, FileSize)
select d.DELIVERABLEUNITREF, f.FILEREF, d.CATALOGUEREFERENCE, d.DMETADATAREF, d.Code, d.SECURITYTAG, dm.MANIFESTATIONREF, f.METADATAREF, f.FILELOCATION, f.NAME,
	(select concat('[', group_concat(json_object('alg', a.ALGORITHMNAME, 'checksum', i.FIXITYVALUE)),']') from digitalfilefixityinfo i
	join fixityalgorithm a on a.FIXITYALGORITHMREF = i.FIXITYALGORITHMREF
	where i.FILEREF = f.FILEREF), f.FILESIZE
from digitalfile f
join manifestationfile m on m.FILEREF = f.FILEREF
join deliverableunitmanifestation dm on dm.MANIFESTATIONREF = m.MANIFESTATIONREF
join tempdu d on d.DELIVERABLEUNITREF = dm.DELIVERABLEUNITREF
where f.DELETED = 'F' and f.EXTANT = 'T' and f.DIRECTORY = 'F' and
    dm.DELETED = 'F' and (dm.ORIGINALITY = 'T' or d.Code != 'WO 409');

create index dufile_ix on dufile (Code);
create index dufile_ix2 on dufile (DELIVERABLEUNITREF);
