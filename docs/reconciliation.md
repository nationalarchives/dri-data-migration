# Data reconciliation

- [Diagram](#diagram)
- [Examples](#examples)
- [Mapping](reconciliation-mapping.md)

Runs the reconciliation process over the extracted and transformed data (staging database). It compares data against three different types of sources:
- metadata CSV file
- closure CSV file
- Discover API

Command: `reconcile`

## Diagram

```mermaid
sequenceDiagram
    actor User
    participant IHostedService
    participant IDataComparison
    participant IReconciliationSource
    participant MetadataSource
    participant ClosureSource
    participant DiscoverySource
    participant IStagingReconciliationClient
    participant StagingRDF@{ "type" : "database" }
    User-)IHostedService: Execute reconcile command
    IHostedService-)IDataComparison: Reconciliation started
    IDataComparison-)IReconciliationSource: Fetch data from the source
    alt Metadata CSV
        IReconciliationSource-)MetadataSource:
    else Closure CSV
        IReconciliationSource-)ClosureSource:
    else Discovery API
        IReconciliationSource-)DiscoverySource:
    end
    IReconciliationSource-)IDataComparison: Transformed source entities
    loop Compare data
        IDataComparison-)IStagingReconciliationClient: Extract
        IStagingReconciliationClient-)StagingRDF: Fetch
        IStagingReconciliationClient-)IDataComparison: Transformed entities
        Note right of IDataComparison: Run comparison
    end
```

## Examples

Metadata CSV file:
```cmd
.\Migration.exe reconcile --reference "XYZ 123" --mapping Metadata --reconciliation-file "c:\XYZ\metadata.csv"
```

Multiple closure CSV files:
```cmd
.\Migration.exe reconcile --reference "XYZ 123" --mapping Closure --reconciliation-file "c:\XYZ\1\closure.csv" --reconciliation-file "c:\XYZ\2\closure.csv"
```

Discovery API (default URI and custom page size):
```cmd
.\Migration.exe reconcile --reference "XYZ 123" --mapping Discovery --page-size 2000
```

Debug with `Migration/Properties/launchSettings.json`
```json
{
    "profiles": {
        "reconciliation": {
            "commandName": "Project",
            "commandLineArgs": "reconcile --reference \"XYZ 123\" --mapping Metadata --reconciliation-file \"c:\\XYZ_123\""
        }
    }
}
```
