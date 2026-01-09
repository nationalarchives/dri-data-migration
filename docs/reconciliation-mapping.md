# Reconciliation mapping

Data is compared only on the condition of source having value for the given field.

## Namespaces

- ex: [http://id.example.com/schema/](https://github.com/nationalarchives/dri-data-migration/blob/main/Ontology.ttl)
- rdf: http://www.w3.org/1999/02/22-rdf-syntax-ns#

## Metadata CSV

Data from the staging triplestore: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Reconciliation/Sparql/ReconciliationPreservicaMetadata.sparql)

| Name | Source | Staging |
| --- | --- | --- |
| Location | identifier | (ex:subsetHasRetention/ex:importLocation or series code) or (ex:assetHasRetention/ex:importLocation and (ex:variationAlternativeName or ex:variationName) or series code) |
| Name | file_name | (only if `file`) ex:variationAlternativeName or ex:variationName |
| FileFolder | folder | rdf:type (ex:Subset or ex:Variation) |
| ModifiedAt | date_last_modified | (only if `file`) ex:assetAlternativeModifiedAt\|ex:assetModifiedAt |
| CoveringDateEnd | end_date | (only if `file`) ex:assetHasOriginDateEnd\|ex:assetAlternativeModifiedAt\|ex:assetHasAlternativeModifiedDateEnd\|ex:assetModifiedAt |

## Closure CSV

Data from the staging triplestore: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Reconciliation/Sparql/ReconciliationPreservicaClosure.sparql)

| Name | Source | Staging |
| --- | --- | --- |
| Location | identifier | (ex:subsetHasRetention/ex:importLocation or series code) or (ex:assetHasRetention/ex:importLocation or series code and (ex:variationAlternativeName or ex:variationName)) |
| FileFolder | folder | ex:Subset or ex:Variation |
| AccessConditionName | closure_type | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionName |
| RetentionType | retention_type | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionName |
| ClosurePeriod | closure_period | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDuration |
| ClosureStartDate | closure_start_date | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionCalculationStartDate |
| FoiExemptionReference | foi_exemption_code | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasLegislation/ex:legislationSectionReference |
| FoiAssertedDate | foi_exemption_asserted | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewDate |
| InstrumentNumber | RI_number | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasRetentionRestriction/ex:retentionInstrumentNumber |
| InstrumentSignedDate | RI_signed_date | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasRetentionRestriction/ex:retentionInstrumentSignatureDate |
| GroundForRetentionCode | retention_justification | (only if `file`) ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasRetentionRestriction/ex:retentionRestrictionHasGroundForRetention/ex:groundForRetentionCode |
| IsPublicName | title_public | (ex:variationHasSensitivityReview\|ex:assetHasSensitivityReview\|ex:subsetHasSensitivityReview)/ex:sensitivityReviewSensitiveName |
| SensitiveName | title_alternate | (ex:variationHasSensitivityReview\|ex:assetHasSensitivityReview\|ex:subsetHasSensitivityReview)/ex:sensitivityReviewSensitiveName |
| IsPublicDescription | description_public | (ex:variationHasSensitivityReview\|ex:assetHasSensitivityReview\|ex:subsetHasSensitivityReview)/ex:sensitivityReviewSensitiveDescription |
| SensitiveDescription | description_alternate | (ex:variationHasSensitivityReview\|ex:assetHasSensitivityReview\|ex:subsetHasSensitivityReview)/ex:sensitivityReviewSensitiveDescription |

## Discovery API

Data from the staging triplestore: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Reconciliation/Sparql/ReconciliationDiscovery.sparql)

| Name | Source | Staging |
| --- | --- | --- |
|Id | Id | ex:assetDriId |
|Name | Title | ex:variationHasSensitivityReview/ex:sensitivityReviewSensitiveName or ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveName or ex:assetName |
|Reference | Reference | ex:assetReference |
|CoveringDateStart | NumStartDate | ex:assetHasOriginDateStart (ex:year, ex:month, ex:day) |
|CoveringDateEnd | NumEndDate | ex:assetHasOriginDateEnd (ex:year, ex:month, ex:day) |
|ClosureStatus | ClosureStatus | ex:variationHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionCode |
|AccessConditionCode | ClosureType | ex:variationHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionCode |
|HeldBy | HeldBy | ex:assetHasRetention/ex:retentionHasFormalBody/ex:formalBodyName |
|ClosurePeriod | ClosureCode | ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDuration |
|ClosureEndYear | ClosureCode | ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionEndYear |
