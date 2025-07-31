using Api;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Help;

namespace Migration;

public class ProgramCommandLineProvider : ConfigurationProvider
{
    private static readonly Option<string> reference = new("--reference", "-ref")
    {
        Description = "Catalogue reference",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };
    private static readonly Option<string> driSparql = new("--dri-sparql", "-ds")
    {
        Description = "DRI query SPARQL endpoint",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };
    private static readonly Option<string> sparql = new("--sparql", "-sq")
    {
        Description = "Query SPARQL endpoint",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };
    private static readonly Option<string> sparqlUpdate = new("--sparql-update", "-su")
    {
        Description = "Update SPARQL endpoint. By default appends 'statements' path segment to the read only SAPRQL endpoint.",
        DefaultValueFactory = a => new Uri(new Uri(a.GetRequiredValue(sparql)), "statements").ToString(),
        Arity = ArgumentArity.ExactlyOne,
        Required = false,
    };
    private static readonly Option<int> pageSize = new("--page-size", "-ps")
    {
        Description = "Allows to provide the page size for queries that needs paging.",
        DefaultValueFactory = _ => 500,
        Arity = ArgumentArity.ExactlyOne,
        Required = false,
    };
    private static readonly Option<string> filePrefix = new("--prefix", "-px")
    {
        Description = "Starting part of of the identifier in the file. This value will be used to replace catalogue reference in the staging data with the reference to match records.",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };
    private static readonly Option<FileInfo> fileLocation = new("--exported-file", "-ef")
    {
        Description = "Location of the exported file.",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };
    private static readonly Option<MapType> mapType = new("--file-type", "-ft")
    {
        Description = "Type of the exported file. Acceptable values: 'Metadata' or 'Closure'.",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };
    private readonly IEnumerable<string> args;

    public ProgramCommandLineProvider(IEnumerable<string> args)
    {
        this.args = args;
        fileLocation.AcceptExistingOnly();
    }

    public override void Load()
    {
        var migrationParse = ParseMigrationCommand();
        var reconciliationParse = ParseReconciliationCommand();

        Data = migrationParse.Count != 0 ? migrationParse : reconciliationParse;
    }

    private static Command MigrationCommand()
    {
        var command = new Command("migrate", """
            Performs data migration from a specified source supporting SPARQL 1.1 Protocol.
            Use '--sparql' and optionally '--sparql-update' to provide SPARQL endpoints.
            Data is partitioned by catalogue reference. Use '--reference' to provide the reference.
            """);

        command.Add(reference);
        command.Add(driSparql);
        command.Add(sparql);
        command.Add(sparqlUpdate);
        command.Add(pageSize);

        return command;
    }

    private Dictionary<string, string?> ParseMigrationCommand()
    {
        var data = new Dictionary<string, string?>();
        var command = MigrationCommand();
        ParseResult parseResult;
        try
        {
            parseResult = command.Parse(args.ToArray());
        }
        catch
        {
            return data; //TODO
        }

        if (parseResult.Errors.Count == 0 && parseResult.UnmatchedTokens.Count == 0)
        {
            if (parseResult.GetValue(reference) is string code)
            {
                data.Add($"{StagingSettings.Prefix}:{nameof(StagingSettings.Code)}", code);
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.Code)}", code);
            }
            if (parseResult.GetValue(driSparql) is string txtDri &&
                    Uri.TryCreate(txtDri, UriKind.Absolute, out var driUri))
            {
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.SparqlConnectionString)}", driUri.ToString());
            }
            if (parseResult.GetValue(sparql) is string txtSparql &&
                Uri.TryCreate(txtSparql, UriKind.Absolute, out var uri))
            {
                data.Add($"{StagingSettings.Prefix}:{nameof(StagingSettings.SparqlConnectionString)}", uri.ToString());
                if (parseResult.GetValue(sparqlUpdate) is string txtUpdate &&
                    Uri.TryCreate(txtUpdate, UriKind.Absolute, out var updateUri))
                {
                    data.Add($"{StagingSettings.Prefix}:{nameof(StagingSettings.SparqlUpdateConnectionString)}", updateUri.ToString());
                }
                else
                {
                    data.Add($"{StagingSettings.Prefix}:{nameof(StagingSettings.SparqlUpdateConnectionString)}", uri.ToString());
                }
            }
            if (parseResult.GetValue(pageSize) is int size)
            {
                data.Add($"{StagingSettings.Prefix}:{nameof(StagingSettings.FetchPageSize)}", size.ToString());
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.FetchPageSize)}", size.ToString());
            }
        }

        return data;
    }

    private static Command ReconciliationCommand()
    {
        var command = new Command("reconcile", """
            Performs reconciliation on migrated data in a staging triplestore against provided file.
            Use '--exported-file' to specify the location of the file exported from Preservica.
            Use '--file-type' to pick the type of the exported file ('Metadata' or 'Closure').
            Use '--prefix' to set the prefix of the identifier in the exported file (eg. 'file:/<REFERENCE>/content').
            Use '--sparql' to provide SPARQL endpoints.
            Use '--reference' to provide the catalogue reference to scope reconciliation process.
            """);

        command.Add(reference);
        command.Add(filePrefix);
        command.Add(fileLocation);
        command.Add(mapType);
        command.Add(sparql);
        command.Add(pageSize);

        return command;
    }

    private Dictionary<string, string?> ParseReconciliationCommand()
    {
        var data = new Dictionary<string, string?>();
        var command = ReconciliationCommand();
        ParseResult parseResult;
        try
        {
            parseResult = command.Parse(args.ToArray());
        }
        catch (Exception ex)
        {
            return data; //TODO
        }

        if (parseResult.Errors.Count == 0 && parseResult.UnmatchedTokens.Count == 0)
        {
            if (parseResult.GetValue(reference) is string code)
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.Code)}", code);
            }
            if (parseResult.GetValue(fileLocation) is FileInfo info)
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.FileLocation)}", info.FullName);
                if (parseResult.GetValue(mapType) is MapType mapKind)
                {
                    data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.MapKind)}", mapKind.ToString());
                }
                if (parseResult.GetValue(filePrefix) is string prefix)
                {
                    data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.FilePrefix)}", prefix);
                }
            }
            if (parseResult.GetValue(pageSize) is int size)
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.FetchPageSize)}", size.ToString());
            }
            if (parseResult.GetValue(sparql) is string txtSparql &&
                Uri.TryCreate(txtSparql, UriKind.Absolute, out var uri))
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.SparqlConnectionString)}", uri.ToString());
            }
        }

        return data;
    }

    public static void PrintHelp()
    {
        var root = new RootCommand();
        root.Add(MigrationCommand());
        root.Add(ReconciliationCommand());

        var result = root.Parse([]);
        new HelpAction().Invoke(result);
    }
}
