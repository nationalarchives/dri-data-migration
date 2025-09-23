using System;

namespace Api;

public record DriChange(string Id, string Table, string Reference, DateTimeOffset Timestamp,
    string UserName, string FullName, string Diff) : IDriRecord;
