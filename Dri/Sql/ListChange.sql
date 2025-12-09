select a.CHANGEREF from auditchange a
join tableinvolved t on t.TABLEINVOLVEDREF = a.TABLEINVOLVEDREF
join dufile d on (d.DELIVERABLEUNITREF = a.PRIMARYKEYVALUE and t.TABLENAME = 'DeliverableUnit') or
    (d.FILEREF = a.PRIMARYKEYVALUE and t.TABLENAME = 'DigitalFile')
where a.XMLDIFF is not null and instr(username, 'du') = 0 and d.Code = $id
order by a.CHANGEREF
