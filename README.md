# Migration

[![Ontology CI](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ontology-test.yml/badge.svg)](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ontology-test.yml)
[![CI](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ci.yml/badge.svg)](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ci.yml)
[![Publish](https://github.com/nationalarchives/dri-data-migration/actions/workflows/publish.yml/badge.svg)](https://github.com/nationalarchives/dri-data-migration/actions/workflows/publish.yml)

- [Staging data model](#staging-data-model)
- [Logging](#logging)
- [Docker](#docker)
- [Setup](#setup)
- [Migration](docs/etl.md)
- [Reconciliation](docs/reconciliation.md)
- [Export](docs/export.md)
- [License](https://github.com/nationalarchives/dri-data-migration/blob/main/LICENSE)

This repo provides tools to extract data from the National Archives DRI (Digital Records Infrastructure) data sources, transform it, and load it into the staging graph database, then generate JSON files.\
To view all available commands and options, run:
```cmd
Migration.exe --help
```

## Staging data model

Digital preservation ontology. It is a simplified model that serves ingestion process only.\
Visualization at [WebVOWL](https://service.tib.eu/webvowl/#iri=https://raw.githubusercontent.com/nationalarchives/dri-data-migration/refs/heads/main/Ontology.ttl).

## Logging

Uses [Serilog](https://serilog.net/) for structured logging implementation. Default logging configuration is defined in the [appsettings.json](https://github.com/nationalarchives/dri-data-migration/blob/main/Migration/appsettings.json) file.\
Default location for the log output is `Console`, `OpenTelemetry` (http://localhost:4317) and `logs` folder.

## Docker

Default configuration is the [compose file](https://github.com/nationalarchives/dri-data-migration/blob/main/compose.yaml). Includes [GraphDB](https://graphdb.ontotext.com/documentation/11.0/) database, to host both the DRI and staging repositories. Additionally, it provides an optional [Open Telemetry dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview).\
To start, run:
```cmd
docker compose up
```

## Setup

Data migration operates on two data sources: SQLite and graph database. [A new table must be created](https://github.com/nationalarchives/dri-data-migration/blob/main/PostSqliteExport.sql) to optimize query performance.\
Both the target (staging) and source (DRI) triplestores can be [hosted using Docker](https://github.com/nationalarchives/dri-data-migration/blob/main/compose.yaml).

1. Download the DRI triplestore backup.
2. Export a subset of SQL data to a SQLite database.
3. [Apply script](https://github.com/nationalarchives/dri-data-migration/blob/main/PostSqliteExport.sql).
4. [Start Docker container](https://github.com/nationalarchives/dri-data-migration/blob/main/compose.yaml).
5. Create the `dri` graph repository.
6. Import the DRI triplestore backup into the `dri` repository.
7. Set the `dri` repository to `read-only`.
8. [Create the `staging` graph repository](https://github.com/nationalarchives/dri-data-migration/blob/main/staging.http#L3).
9. [Apply the ontology](https://github.com/nationalarchives/dri-data-migration/blob/main/staging.http#L53).
