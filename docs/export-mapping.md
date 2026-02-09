# Export mapping

## Namespaces

ex: [http://id.example.com/schema/](https://github.com/nationalarchives/dri-data-migration/blob/main/Ontology.ttl)

## JSON

Data from the staging database: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Exporter/Sparql/ExportJson.sparql)\
JSON schema: [file](export-json-schema.json)

| JSON | Staging |
| --- | --- |
| RecordId | ex:variationDriManifestationId |
| IaId | (ex:wo409SubsetDriId\|ex:assetDriId) and ex:redactedVariationSequence\|ex:presentationVariationSequence |
| Reference | ex:assetReference and ex:redactedVariationSequence\|ex:presentationVariationSequence |
| Title | ex:assetName |
| TranslatedTitle | ex:assetAlternativeName |
| PublicTitle | ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveName or ex:variationHasSensitivityReview/ex:sensitivityReviewSensitiveName or ex:assetName |
| CuratedTitle | first ex:assetHasVariation/ex:variationAlternativeName |
| Description | ex:assetDescription |
| PublicDescription | ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveDescription or ex:variationHasSensitivityReview/ex:sensitivityReviewSensitiveDescription or ex:assetDescription |
| FormerReferenceTna | ex:assetPastReference |
| FormerReferenceDepartment | ex:assetPreviousReference |
| Summary | ex:assetSummary |
| Tag | ex:assetTag |
| Arrangement | Iterate over ex:assetHasSubset/ex:subsetHasBroaderSubset/ex:importLocation |
| PublicArrangement | Iterate over ex:assetHasSubset/ex:subsetHasBroaderSubset/ex:importLocation and replace segment if ex:subsetHasSensitivityReview/ex:sensitivityReviewSensitiveName exists |
| TdrConsignmentId | ex:consignmentTdrReference |
| TdrFileReference | ex:fileTdrReference |
| TdrParentReference | ex:parentTdrReference |
| TdrUuid | ex:assetTdrId |
| DriBatchReference | ex:batchDriId |
| SourceInternalName | ex:assetSourceInternalName |
| ConnectedAssetNote | ex:assetConnectedAssetNote |
| PhysicalDescription | ex:assetPhysicalDescription |
| PaperNumber | ex:paperNumber |
| PoorLawUnionNumber | ex:poorLawUnionNumber |
| UsageRestrictionDescription | ex:assetUsageRestrictionDescription |
| UkGovernmentWebArchive | ex:assetHasUkGovernmentWebArchive |
| LegalStatus | Last URI segment of ex:assetHasLegalStatus |
| RecordType | Last URI segment of ex:assetHasAssetTagType |
| Language | ex:assetHasLanguage/ex:languageName |
| CopyrightHolders | ex:assetHasCopyright/ex:copyrightTitle |
| HeldBy | ex:assetHasRetention/ex:retentionHasFormalBody/ex:formalBodyName |
| CreatedBy | ex:assetHasCreation/ex:creationHasFormalBody/ex:formalBodyName |
| TransferredBy | ex:assetHasTransfer/ex:transferHasFormalBody/ex:formalBodyName |
| DateLastModified  | ex:assetModifiedAt |
| CuratedModifiedAt | ex:assetAlternativeModifiedAt |
| CuratedDateStart | ex:assetHasAlternativeModifiedDateStart (ex:year, ex:month, ex:day) |
| CuratedDateEnd | ex:assetHasAlternativeModifiedDateEnd (ex:year, ex:month, ex:day) |
| CuratedModifiedAtNote | ex:assetAlternativeModifiedAtNote |
| GeographicalPlace | ex:assetHasAssociatedGeographicalPlace/ex:geographicalPlaceName |
| CoveringDateStart | full date ex:assetHasOriginDateStart (ex:year, ex:month, ex:day)\|ex:assetHasOriginApproximateDateStart (ex:year, ex:month, ex:day)\|ex:assetModifiedAt |
| CoveringDateEnd | full date ex:assetHasOriginDateEnd (ex:year, ex:month, ex:day)\|ex:assetHasOriginApproximateDateEnd (ex:year, ex:month, ex:day)\|ex:assetModifiedAt |
| ProvidedCoveringDateStart | ex:assetHasOriginDateStart (ex:year, ex:month, ex:day)\|ex:assetHasOriginApproximateDateStart (ex:year, ex:month, ex:day) |
| ProvidedCoveringDateEnd | ex:assetHasOriginDateEnd (ex:year, ex:month, ex:day)\|ex:assetHasOriginApproximateDateEnd (ex:year, ex:month, ex:day) |
| ProvidedCoveringDateText | ex:assetHasOriginDateStart/ex:dateVerbatim\|ex:assetHasOriginApproximateDateStart/ex:dateVerbatim |
| FilmProductionCompanyName | ex:filmProductionCompanyName |
| FilmTitle | ex:filmTitle |
| FilmDuration | ex:filmDuration |
| EvidenceProvider | ex:inquiryAssetHasEvidenceProvider/ex:evidenceProviderName |
| Investigations | ex:inquiryAssetHasInquiryInvestigation/ex:inquiryInvestigationName |
| InquiryHearingDate | ex:inquiryHearingDate |
| InquirySessionDescription | ex:inquirySessionDescription |
| InquiryAppearances.Sequence | ex:inquiryAssetHasInquiryAppearance/ex:inquiryAppearanceSequence |
| InquiryAppearances.WitnessNames | ex:inquiryAssetHasInquiryAppearance/ex:inquiryAppearanceHasInquiryWitness/ex:inquiryWitnessName |
| InquiryAppearances.AppearanceDescription | ex:inquiryAssetHasInquiryAppearance/ex:inquiryWitnessAppearanceDescription |
| CourtSession | ex:courtSessionDescription |
| CourtSessionDate | ex:courtSessionDate |
| CourtCases.Sequence | ex:courtAssetHasCourtCase/ex:courtCaseSequence |
| CourtCases.Name | ex:courtAssetHasCourtCase/ex:courtCaseName |
| CourtCases.Reference | ex:courtAssetHasCourtCase/ex:courtCaseReference |
| CourtCases.Summary | ex:courtAssetHasCourtCase/ex:courtCaseSummary |
| CourtCases.SummaryJudgment | ex:courtAssetHasCourtCase/ex:courtCaseSummaryJudgment |
| CourtCases.SummaryReasonsForJudgment | ex:courtAssetHasCourtCase/ex:courtCaseSummaryReasonsForJudgment |
| CourtCases.HearingStartDate | ex:courtAssetHasCourtCase/ex:courtCaseHearingStartDate |
| CourtCases.HearingEndDate | ex:courtAssetHasCourtCase/ex:courtCaseHearingEndDate |
| SealOwnerName | ex:sealOwnerName |
| SealColour | ex:sealColour |
| EmailAttachmentReference | ex:emailAttachmentReference |
| SealCategory | ex:sealAssetHasSealCategory/ex:sealCategoryName |
| ImageSequenceEnd | ex:imageSequenceStart |
| ImageSequenceStart | ex:imageSequenceEnd |
| DimensionMm.First | ex:assetHasDimension/ex:firstDimensionMillimetre |
| DimensionMm.Second | ex:assetHasDimension/ex:secondDimensionMillimetre |
| DimensionMm.IsFragment | Set to true if ex:assetHasDimension's object is ex:FragmentDimension |
| ObverseDimensionMm.First | ex:sealAssetHasObverseDimension/ex:firstDimensionMillimetre |
| ObverseDimensionMm.Second | ex:sealAssetHasObverseDimension/ex:secondDimensionMillimetre |
| ObverseDimensionMm.IsFragment | Set to true if ex:sealAssetHasObverseDimension's object is ex:FragmentDimension |
| ReverseDimensionMm.First | ex:sealAssetHasReverseDimension/ex:firstDimensionMillimetre |
| ReverseDimensionMm.Second | ex:sealAssetHasReverseDimension/ex:secondDimensionMillimetre |
| ReverseDimensionMm.IsFragment | Set to true if ex:sealAssetHasReverseDimension's object is ex:FragmentDimension |
| SealStartDate | ex:sealAssetHasStartDate (ex:year, ex:month, ex:day) |
| SealEndDate | ex:sealAssetHasEndDate (ex:year, ex:month, ex:day) |
| SealObverseStartDate | ex:sealAssetHasObverseStartDate (ex:year, ex:month, ex:day) |
| SealObverseEndDate | ex:sealAssetHasObverseEndDate (ex:year, ex:month, ex:day) |
| SealReverseStartDate | ex:sealAssetHasReverseStartDate (ex:year, ex:month, ex:day) |
| SealReverseEndDate | ex:sealAssetHasReverseEndDate (ex:year, ex:month, ex:day) |
| Address | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasContactPoint/ex:geographicalPlaceName |
| BattalionName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasBattalionMembership/ex:battalionMembershipHasBattalion/ex:battalionName |
| BirthAddress | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasBirthAddress/ex:geographicalPlaceName |
| DateOfBirth | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasDateOfBirth (ex:year, ex:month, ex:day) |
| FamilyName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personFamilyName |
| FullName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personFullName |
| GivenName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personGivenName |
| IsVeteran | ex:assetHasVeteran exists |
| NextOfKinName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasNextOfKinRelationship/ex:nextOfKinRelationshipHasNextOfKin/ex:personFullName |
| NextOfKinTypes | Last URI segments of (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasNextOfKinRelationship/ex:nextOfKinRelationshipHasKinship |
| SeamanServiceNumber | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:seamanServiceNumber |
| Note | first ex:assetHasVariation/ex:variationNote |
| PhysicalConditionDescription | first ex:assetHasVariation/ex:variationPhysicalConditionDescription |
| ReferenceGoogleId | first ex:assetHasVariation/ex:variationReferenceGoogleId |
| ReferenceParentGoogleId | first ex:assetHasVariation/ex:variationReferenceParentGoogleId |
| ArchivistNote | first ex:assetHasVariation/ex:variationHasDatedNote/ex:archivistNote |
| ArchivistNoteDate | first ex:assetHasVariation/ex:variationHasDatedNote/(ex:archivistNoteAt or ex:datedNoteHasDate (ex:year, ex:month, ex:day)) |
| NationalRegistrationNumber | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:nationalRegistrationNumber |
| Sensitivity.HasSensitiveMetadata | `true` when ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveName (at any level in the tree) or ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveDescription |
| Sensitivity.FoiAssertedDate | ex:assetHasSensitivityReview/ex:sensitivityReviewDate |
| Sensitivity.SensitiveName | ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveName |
| Sensitivity.SensitiveDescription | ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveDescription |
| Sensitivity.AccessConditionName | ex:assetHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionName |
| Sensitivity.AccessConditionCode | ex:assetHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionCode |
| Sensitivity.ClosureReviewDate | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionReviewDate |
| Sensitivity.ClosureStartDate | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionCalculationStartDate |
| Sensitivity.ClosurePeriod | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDuration |
| Sensitivity.ClosureEndYear | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionEndYear |
| Sensitivity.FoiExemptions | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasLegislation |
| Sensitivity.InstrumentNumber | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionInstrumentNumber |
| Sensitivity.InstrumentSignedDate | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionInstrumentSignatureDate |
| Sensitivity.RetentionReconsiderDate | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionRestrictionReviewDate |
| Sensitivity.GroundForRetentionCode | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:groundForRetentionCode |
| Sensitivity.GroundForRetentionDescription | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:groundForRetentionDescription |
| AuditTrail.DescriptionBase64 | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeDescription |
| AuditTrail.Timestamp | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeDateTime |
| AuditTrail.OperatorName | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeHasOperator/ex:operatorName |
| AuditTrail.Reason | ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDescription |
| AuditTrail.Sensitivity.FoiAssertedDate.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewDate |
| AuditTrail.Sensitivity.SensitiveName.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveName |
| AuditTrail.Sensitivity.SensitiveDescription.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveDescription |
| AuditTrail.Sensitivity.AccessConditionName.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionName |
| AuditTrail.Sensitivity.AccessConditionCode.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasAccessCondition/ex:accessConditionCode |
| AuditTrail.Sensitivity.ClosureReviewDate.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionReviewDate |
| AuditTrail.Sensitivity.ClosureStartDate.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionCalculationStartDate |
| AuditTrail.Sensitivity.ClosurePeriod.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDuration |
| AuditTrail.Sensitivity.ClosureEndYear.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionEndYear |
| AuditTrail.Sensitivity.FoiExemptions.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasLegislation |
| AuditTrail.Sensitivity.InstrumentNumber.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionInstrumentNumber |
| AuditTrail.Sensitivity.InstrumentSignedDate.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionInstrumentSignatureDate |
| AuditTrail.Sensitivity.RetentionReconsiderDate.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionRestrictionReviewDate |
| AuditTrail.Sensitivity.GroundForRetentionCode.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:groundForRetentionCode |
| AuditTrail.Sensitivity.GroundForRetentionDescription.{Value and/or NewValue} | Compare with following ex:assetHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:groundForRetentionDescription |
| DigitalFileCount | Count of grouped variations |
| DigitalFiles.FileId | ex:variationDriId |
| DigitalFiles.FileName | ex:assetHasVariation/(ex:variationAlternativeName or ex:variationName) |
| DigitalFiles.SizeBytes | ex:assetHasVariation/ex:variationSizeBytes |
| DigitalFiles.Checksums.Hash | ex:assetHasVariation/variationHasVariationDataIntegrityCalculation/ex:variationDataIntegrityCalculationHasHashFunction |
| DigitalFiles.Checksums.Value | ex:assetHasVariation/ex:variationHasVariationDataIntegrityCalculation/ex:checksum |
| DigitalFiles.SortOrder | ex:assetHasVariation/ex:variationSequence |
| DigitalFiles.Sequence | ex:assetHasVariation/(ex:redactedVariationSequence\|ex:presentationVariationSequence) |
| DigitalFiles.FilePath | ex:assetHasVariation/ex:variationRelativeLocation |
| DigitalFiles.ScannerOperatorIdentifier | ex:assetHasVariation/ex:scannerOperatorIdentifier |
| DigitalFiles.ScannerIdentifier | ex:assetHasVariation/ex:scannerIdentifier |
| DigitalFiles.ScannerGeographicalPlace | ex:assetHasVariation/ex:scannedVariationHasScannerGeographicalPlace/ex:geographicalPlaceName |
| DigitalFiles.ScannedImageCrop | Last URI segment of ex:assetHasVariation/ex:scannedVariationHasImageCrop |
| DigitalFiles.ScannedImageDeskew | Last URI segment of ex:assetHasVariation/ex:scannedVariationHasImageDeskew |
| DigitalFiles.ScannedImageSplit | Last URI segment of ex:assetHasVariation/ex:scannedVariationHasImageSplit |
| Relationships.Relationship | Depending on existence of ex:assetRelationReference|ex:assetRelationIdentifier, ex:assetConnectedAssetNote or ex:assetHasVariation/ex:redactedVariationSequence or ex:assetHasVariation/ex:presentationVariationSequence |
| Relationships.Reference | ex:assetRelationReference|ex:assetRelationIdentifier or ex:assetConnectedAssetNote or ex:assetReference and ex:assetHasVariation/ex:redactedVariationSequence or ex:assetHasVariation/ex:presentationVariationSequence|
| Relationships.RelationDescription | ex:assetRelationDescription |

## XML

Data from the staging database: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Exporter/Sparql/ExportXml.sparql)

Every object associated with the following predicates is written to its own XML file:
- ex:assetDriXml
- ex:wo409SubsetDriXml
- ex:variationDriXml
