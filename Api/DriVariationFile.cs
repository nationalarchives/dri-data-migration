﻿namespace Api;

public record DriVariationFile(string Id, string Location, string Name, string ManifestationId, string Xml)
    : IDriRecord;
