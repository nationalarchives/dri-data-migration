using Api;
using Microsoft.Extensions.Configuration;
using System.CommandLine;

namespace Migration;

public class ProgramCommandLineProvider : ConfigurationProvider
{
    private static readonly Option<string> reference = new("--reference", "-ref")
    {
        Description = "Specifies the reference identifier for the catalogue.",
        Arity = ArgumentArity.ExactlyOne,
        Required = true
    };
    private static readonly Option<string> sql = new("--sql")
    {
        Description = "Defines the SQLite connection string.",
        DefaultValueFactory = _ => "Data Source=dri.sqlite;Mode=ReadOnly",
        Arity = ArgumentArity.ExactlyOne,
        Required = true
    };
    private static readonly Option<Uri> driSparql = new("--dri-sparql", "-ds")
    {
        Description = "Sets the SPARQL endpoint for querying the DRI repository.",
        DefaultValueFactory = _ => new Uri("http://localhost:7200/repositories/dri"),
        Arity = ArgumentArity.ExactlyOne,
        Required = false,
        CustomParser = UriArgumentParser()
    };
    private static readonly Option<Uri> sparql = new("--sparql", "-sq")
    {
        Description = "Sets the SPARQL endpoint for querying the staging repository.",
        DefaultValueFactory = _ => new Uri("http://localhost:7200/repositories/staging"),
        Arity = ArgumentArity.ExactlyOne,
        Required = false,
        CustomParser = UriArgumentParser()
    };
    private static readonly Option<Uri> sparqlUpdate = new("--sparql-update", "-su")
    {
        Description = "Sets the SPARQL endpoint for updating the staging repository.",
        DefaultValueFactory = a => new Uri("http://localhost:7200/repositories/staging/statements"),
        Arity = ArgumentArity.ZeroOrOne,
        Required = false,
        CustomParser = UriArgumentParser()
    };
    private static readonly Option<int> pageSize = new("--page-size", "-ps")
    {
        Description = "Specifies the number of records per page for paginated queries.",
        DefaultValueFactory = _ => 500,
        Arity = ArgumentArity.ZeroOrOne,
        Required = false
    };
    private static readonly Option<EtlStageType?> restartFromStage = new("--restart-from-stage", "-rfs")
    {
        Description = "Used in restart scenarios to specify the migration stage to resume from.",
        Arity = ArgumentArity.ZeroOrOne,
        Required = false
    };
    private static readonly Option<int> restartFromOffset = new("--restart-from-offset", "-rfo")
    {
        Description = "Used in restart scenarios to specify the pagination stage to resume from. Required when running migration and using the --restart-from-stage option.",
        DefaultValueFactory = _ => 0,
        Arity = ArgumentArity.ZeroOrOne,
        Required = false
    };
    private static readonly Option<IEnumerable<string>> fileLocation = new("--reconciliation-file", "-rf")
    {
        Description = "Defines the location of the reconciliation file.",
        Arity = ArgumentArity.ZeroOrMore,
        Required = false
    };
    private static readonly Option<ReconciliationMapType> mapType = new("--mapping", "-mp")
    {
        Description = "Defines the type of reconciliation.",
        Arity = ArgumentArity.ExactlyOne,
        Required = true
    };
    private static readonly Option<string> discoveryRecordsUri = new("--discovery-uri", "-du")
    {
        Description = "Specifies the URI for the Discovery API search records endpoint. Default: https://discovery.nationalarchives.gov.uk/API/search/v1/records.",
        DefaultValueFactory = _ => "https://discovery.nationalarchives.gov.uk/API/search/v1/records",
        Arity = ArgumentArity.ExactlyOne,
        Required = false
    };
    private static readonly Option<ExportScopeType?> exportScope = new("--export-scope", "-es")
    {
        Description = "Defines the scope of data for export.",
        DefaultValueFactory = _ => ExportScopeType.JSON,
        Arity = ArgumentArity.ZeroOrOne,
        Required = false
    };

    private static Func<System.CommandLine.Parsing.ArgumentResult, Uri?> UriArgumentParser()
    {
        return argumentResult =>
        {
            if (Uri.TryCreate(argumentResult.Tokens.Single().Value, UriKind.Absolute, out var uri))
            {
                return uri;
            }
            argumentResult.AddError("Invalid URI");
            return null;
        };
    }

    private readonly IEnumerable<string> args;
    private readonly Command MigrateCommand;
    private readonly Command ReconcileCommand;
    private readonly Command ExportCommand;

    public ProgramCommandLineProvider(IEnumerable<string> args)
    {
        this.args = args;
        fileLocation.Validators.Add(result =>
        {
            for (var i = 0; i < result.Tokens.Count; i++)
            {
                var fileName = result.Tokens[i].Value;
                if (!Path.Exists(fileName))
                {
                    result.AddError($"File {fileName} not found");
                }
            }
        });

        MigrateCommand = new Command("migrate", """
            Migrates data from two sources: a SPARQL 1.1-compliant triplestore and an SQLite database.
            """)
        {
            reference,
            sql,
            driSparql,
            sparql,
            sparqlUpdate,
            pageSize,
            restartFromStage,
            restartFromOffset
        };

        ReconcileCommand = new Command("reconcile", """
            Reconciles migrated data in the staging triplestore against a metadata file, closure file, or Discovery API.
            """)
        {
            reference,
            fileLocation,
            mapType,
            sparql,
            pageSize,
            discoveryRecordsUri
        };

        ExportCommand = new Command("export", """
            Exports migrated data to JSON and/or XML file(s).
            """)
        {
            reference,
            exportScope,
            sparql,
            pageSize,
            restartFromOffset
        };
    }

    public override void Load()
    {
        var migrationParse = ParseMigrationCommand();
        var reconciliationParse = ParseReconciliationCommand();
        var exportParse = ParseExportCommand();

        if (migrationParse.Count != 0)
        {
            Data = migrationParse;
            Data.Add("app:command", MigrateCommand.Name);
        }
        else if (reconciliationParse.Count != 0)
        {
            Data = reconciliationParse;
            Data.Add("app:command", ReconcileCommand.Name);
        }
        else if (exportParse.Count != 0)
        {
            Data = exportParse;
            Data.Add("app:command", ExportCommand.Name);
        }

        if (!Data.Any())
        {
            PrintHelp();
        }
    }

    private Dictionary<string, string?> ParseMigrationCommand()
    {
        var data = new Dictionary<string, string?>();
        ParseResult? parseResult = null;
        try
        {
            parseResult = MigrateCommand.Parse(args.ToArray());
        }
        catch
        {
            return [];
        }

        if (parseResult.Errors.Count == 0 && parseResult.UnmatchedTokens.Count == 0)
        {
            if (parseResult.GetValue(reference) is string code)
            {
                data.Add($"{StagingSettings.Prefix}:{nameof(StagingSettings.Code)}", code);
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.Code)}", code);
            }
            if (parseResult.GetValue(sql) is string sqlConnection)
            {
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.SqlConnectionString)}", sqlConnection);
            }
            if (parseResult.GetValue(driSparql) is Uri driUri)
            {
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.SparqlConnectionString)}", driUri.ToString());
            }
            if (parseResult.GetValue(sparql) is Uri uri)
            {
                data.Add($"{StagingSettings.Prefix}:{nameof(StagingSettings.SparqlConnectionString)}", uri.ToString());
                if (parseResult.GetValue(sparqlUpdate) is Uri updateUri)
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
            if (parseResult.GetValue(restartFromStage) is EtlStageType etlStageType)
            {
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.RestartFromStage)}", etlStageType.ToString());
                data.Add($"{DriSettings.Prefix}:{nameof(DriSettings.RestartFromOffset)}", parseResult.GetValue(restartFromOffset).ToString());
            }
        }

        return data;
    }

    private Dictionary<string, string?> ParseReconciliationCommand()
    {
        var data = new Dictionary<string, string?>();
        ParseResult? parseResult = null;
        try
        {
            parseResult = ReconcileCommand.Parse(args.ToArray());
        }
        catch
        {
            return [];
        }

        if (parseResult.Errors.Count == 0 && parseResult.UnmatchedTokens.Count == 0)
        {
            if (parseResult.GetValue(reference) is string code)
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.Code)}", code);
            }
            if (parseResult.GetValue(mapType) is ReconciliationMapType mapKind)
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.MapKind)}", mapKind.ToString());
            }
            var files = parseResult.GetValue(fileLocation);
            if (files is not null && files.Any())
            {
                for (int i = 0; i < files.Count(); i++)
                {
                    data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.FileLocation)}:{i}", files.ElementAt(i));
                }
            }
            else if (mapKind != ReconciliationMapType.Discovery)
            {
                return [];
            }
            if (parseResult.GetValue(pageSize) is int size)
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.FetchPageSize)}", size.ToString());
            }
            if (parseResult.GetValue(sparql) is Uri sparqlUri)
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.SparqlConnectionString)}", sparqlUri.ToString());
            }
            if (parseResult.GetValue(discoveryRecordsUri) is string txtDiscovery &&
                Uri.TryCreate(txtDiscovery, UriKind.Absolute, out var discoveryUri))
            {
                data.Add($"{ReconciliationSettings.Prefix}:{nameof(ReconciliationSettings.SearchRecordUri)}", discoveryUri.ToString());
            }
        }

        return data;
    }

    private Dictionary<string, string?> ParseExportCommand()
    {
        var data = new Dictionary<string, string?>();
        ParseResult? parseResult = null;
        try
        {
            parseResult = ExportCommand.Parse(args.ToArray());
        }
        catch
        {
            return [];
        }

        if (parseResult.Errors.Count == 0 && parseResult.UnmatchedTokens.Count == 0)
        {
            if (parseResult.GetValue(reference) is string code)
            {
                data.Add($"{ExportSettings.Prefix}:{nameof(ExportSettings.Code)}", code);
            }
            if (parseResult.GetValue(exportScope) is ExportScopeType scopeType)
            {
                data.Add($"{ExportSettings.Prefix}:{nameof(ExportSettings.ExportScope)}", scopeType.ToString());
            }
            if (parseResult.GetValue(sparql) is Uri uri)
            {
                data.Add($"{ExportSettings.Prefix}:{nameof(ExportSettings.SparqlConnectionString)}", uri.ToString());
            }
            if (parseResult.GetValue(pageSize) is int size)
            {
                data.Add($"{ExportSettings.Prefix}:{nameof(ExportSettings.FetchPageSize)}", size.ToString());
            }
            data.Add($"{ExportSettings.Prefix}:{nameof(ExportSettings.RestartFromOffset)}", parseResult.GetValue(restartFromOffset).ToString());
        }

        return data;
    }

    private void PrintHelp()
    {
        var root = new RootCommand
        {
            MigrateCommand,
            ReconcileCommand,
            ExportCommand
        };
        root.Parse(args.ToArray()).Invoke();
    }
}
