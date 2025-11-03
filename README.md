[![Ontology CI](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ontology-test.yml/badge.svg)](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ontology-test.yml)
[![CI](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ci.yml/badge.svg)](https://github.com/nationalarchives/dri-data-migration/actions/workflows/ci.yml)
[![Publish](https://github.com/nationalarchives/dri-data-migration/actions/workflows/publish.yml/badge.svg)](https://github.com/nationalarchives/dri-data-migration/actions/workflows/publish.yml)

# Staging (interim) data model

Schema visualization at [WebVOWL](https://service.tib.eu/webvowl/#iri=https://raw.githubusercontent.com/nationalarchives/dri-data-migration/refs/heads/main/Ontology.ttl)

# DRI (Digital Records Infrastructure) data migration

Migration CLI provides functionality to run sequential, recoverable, idempotent and observable [ETL process](#data-extraction). It also enables [extracted data reconciliation](#data-reconciliation) and [exporting extracted data to JSON files](#data-export).

To view available commands and options, run:
```cmd
Migration.exe --help
```

## Prerequisites

Data migration operates on two data sources: SQLite and triplestore. [A new table must be created](https://github.com/nationalarchives/dri-data-migration/blob/main/PostSqliteExport.sql) to optimize query performance.
Both the target (staging triplestore) and DRI triplestore can be [hosted using Docker](https://github.com/nationalarchives/dri-data-migration/blob/main/compose.yaml).

### Setup

1. Download the DRI triplestore backup.
2. Export a subset of SQL data from to a SQLite database.
3. Apply [script](https://github.com/nationalarchives/dri-data-migration/blob/main/PostSqliteExport.sql).
4. [Start Docker container](https://github.com/nationalarchives/dri-data-migration/blob/main/compose.yaml).
5. Create the `dri` repository.
6. Import backup into the `dri` repository.
7. Set the `dri` repository to `read-only`.
8. [Create the `staging` repository](https://github.com/nationalarchives/dri-data-migration/blob/main/staging.http#L3).
9. [Apply the ontology](https://github.com/nationalarchives/dri-data-migration/blob/main/staging.http#L53).

# Data extraction

Runs the ETL process for a specified series (collection).

Command: `migrate`.

## Examples

All default options:
```cmd
.\Migration.exe migrate --reference "XYZ 123"
```

SQLite file location and page size:
```cmd
.\Migration.exe migrate --reference "XYZ 123" --sql "Data Source=c:/dri.sqlite;Mode=ReadOnly" --page-size 3000
```

Restart from `Sensitivity review` stage at record 100:
```cmd
.\Migration.exe migrate --reference "XYZ 123" --restart-from-stage SensitivityReview --restart-from-offset 100
```

## Sequence

1. Access conditions

   Ingested data is shared across all series.
   [Data comes from the DRI triplestore](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetAccessConditionsAsync.sparql).

2. Legislations

   Ingested data is shared across all series.
   [Data comes from the DRI triplestore](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetLegislationsAsync.sparql).

3. Grounds for retention

   Ingested data is shared across all series.
   [Data comes from the DRI triplestore](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetGroundsForRetentionAsync.sparql).

4. Subsets

   [Data comes from the DRI triplestore](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetSubsetsByCodeAsync.sparql).

5. Assets

   [Data comes from the DRI triplestore](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetAssetsByCodeAsync.sparql).

6. Variations

   [Data comes from the DRI triplestore](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetVariationsByCodeAsync.sparql).

7. Asset (deliverable units)

   Enhances data ingested in step 5 by parsing XML stored in the SQL database.
   [Data comes from the SQLite](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetAssetDeliverableUnits.sql).

8. `WO 409` subset (deliverable units)

   Only applies to `WO 409` series. Enhances data ingested in step 5 and 7 by parsing XML stored in the SQL database.
   [Data comes from the SQLite](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetWo409SubsetDeliverableUnits.sql).

9. Variations (files)

   Enhances data ingested in step 6 by parsing XML stored in the SQL database.
   [Data comes from the SQLite](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetVariationFiles.sql).

10. Sensitivity reviews

    [Data comes from the DRI triplestore](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetSensitivityReviewsByCodeAsync.sparql).

11. Changes

    [Data comes from the SQLite](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetChanges.sql).

# Data reconciliation

Command: `reconcile`.

# Data export

Command: `export`.
