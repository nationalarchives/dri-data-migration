select distinct t.TABLENAME, a.CHANGEREF, a.PRIMARYKEYVALUE, a.DATETIME, a.USERNAME, a.FULLNAME, a.XMLDIFF from auditchange a
join tableinvolved t on t.TABLEINVOLVEDREF = a.TABLEINVOLVEDREF
join dufile d on (d.DELIVERABLEUNITREF = a.PRIMARYKEYVALUE and t.TABLENAME = 'DeliverableUnit') or
    (d.FILEREF = a.PRIMARYKEYVALUE and t.TABLENAME = 'DigitalFile')
where a.XMLDIFF is not null and instr(username, 'du') = 0 and d.Code = $code
order by a.rowid
limit $limit offset $offset