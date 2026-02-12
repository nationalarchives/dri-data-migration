namespace Exporter;

internal static class ReferenceBuilder
{
    internal static string Build(long? redactedSequence, string assetReference)
    {
        string reference;
        if (assetReference.StartsWith("WO/16/409/"))
        {
            reference = assetReference.Replace("/Service/1", string.Empty)
                .Replace("WO/16/409/", "WO 409/").Replace('_', '/') switch
            {
                "WO 409/27/101/668/Medical/1" => "WO 409/27/101/1071",
                "WO 409/27/102/20/Medical/1" => "WO 409/27/102/1059",
                "WO 409/27/102/20/Medical/2" => "WO 409/27/102/1059",
                "WO 409/27/14/345/Medical/1" => "WO 409/27/14/537",
                "WO 409/27/30/300/Medical/1" => "WO 409/27/30/1058",
                "WO 409/27/4/46/Medical/1" => "WO 409/27/4/678",
                "WO 409/27/4/46/Medical/2" => "WO 409/27/4/678",
                "WO 409/27/51/301/Medical/1" => "WO 409/27/51/738",
                "WO 409/27/51/301/Medical/2" => "WO 409/27/51/738",
                "WO 409/27/70/26/Medical/1" => "WO 409/27/70/1074",
                "WO 409/27/70/26/Medical/2" => "WO 409/27/70/1074",
                "WO 409/27/93/12/Medical/1" => "WO 409/27/93/662",
                "WO 409/27/93/12/Medical/2" => "WO 409/27/93/662",
                "WO 409/27/93/169/Medical/1" => "WO 409/27/93/663",
                "WO 409/27/93/169/Medical/2" => "WO 409/27/93/663",
                "WO 409/27/93/278/Medical/1" => "WO 409/27/93/664",
                "WO 409/27/93/278/Medical/2" => "WO 409/27/93/664",
                "WO 409/27/93/319/Medical/1" => "WO 409/27/93/665",
                "WO 409/27/93/319/Medical/2" => "WO 409/27/93/665",
                string txt => txt
            };
        }
        else
        {
            var firstSlash = assetReference.IndexOf('/');
            reference = firstSlash > 0 ? assetReference.Remove(firstSlash, 1).Insert(firstSlash, " ") : assetReference;
        }

        return redactedSequence is null ? reference : $"{reference}/{redactedSequence}";
    }
}
