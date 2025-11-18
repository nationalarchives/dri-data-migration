# Export mapping

## Namespaces

ex: [http://id.example.com/schema/](https://github.com/nationalarchives/dri-data-migration/blob/main/Ontology.ttl)

## JSON

Data from the staging database: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Exporter/Sparql/ExportJson.sparql)

| JSON | Staging |
| --- | --- |
| RecordId | ex:variationDriManifestationId |
| IaId | ex:assetDriId and ex:redactedVariationSequence |
| Reference | ex:assetReference and ex:redactedVariationSequence |
| Title | ex:assetName |
| TranslatedTitle | ex:assetAlternativeName |
| PublishedTitle | ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveName or ex:variationHasSensitivityReview/ex:sensitivityReviewSensitiveName or ex:assetName |
| Description | ex:assetDescription |
| PublishedDescription | ex:assetHasSensitivityReview/ex:sensitivityReviewSensitiveDescription or ex:variationHasSensitivityReview/ex:sensitivityReviewSensitiveDescription or ex:assetDescription |
| PastReference | ex:assetPastReference |
| Summary | ex:assetSummary |
| Tag | ex:assetTag |
| Arrangement | Iterate over ex:assetHasSubset/ex:subsetHasBroaderSubset/ex:importLocation |
| PublishedArrangement | Iterate over ex:assetHasSubset/ex:subsetHasBroaderSubset/ex:importLocation and replace segment if ex:subsetHasSensitivityReview/ex:sensitivityReviewSensitiveName exists |
| ConsignmentId | ex:consignmentTdrId |
| DriBatchReference | ex:batchDriId |
| SourceInternalName | ex:assetSourceInternalName |
| ConnectedAssetNote | ex:assetConnectedAssetNote |
| RelationDescription | ex:assetRelationDescription |
| PhysicalDescription | ex:assetPhysicalDescription |
| PaperNumber | ex:paperNumber |
| UsageRestrictionDescription | ex:assetUsageRestrictionDescription |
| UkGovernmentWebArchive | ex:assetHasUkGovernmentWebArchive |
| LegalStatus | Last URI segemnt of ex:assetHasLegalStatus |
| Language | ex:assetHasLanguage/ex:languageName |
| CopyrightHolders | ex:assetHasCopyright/ex:copyrightTitle |
| HeldBy | ex:assetHasRetention/ex:retentionHasFormalBody/ex:formalBodyName |
| CreatedBy | ex:assetHasCreation/ex:creationHasFormalBody/ex:formalBodyName |
| GeographicalPlace | ex:assetHasAssociatedGeographicalPlace/ex:geographicalPlaceName |
| CoveringDateStart | ex:assetHasOriginDateStart |
| CoveringDateEnd | ex:assetHasOriginDateEnd |
| CoveringApproximateDateStart | ex:assetHasOriginApproximateDateStart |
| CoveringApproximateDateEnd | ex:assetHasOriginApproximateDateEnd |
| FilmProductionCompanyName | ex:filmProductionCompanyName |
| FilmTitle | ex:filmTitle |
| FilmDuration | ex:filmDuration |
| EvidenceProvider | ex:evidenceProviderName |
| Investigation | ex:investigationName |
| InquiryHearingDate | ex:inquiryHearingDate |
| InquirySessionDescription | ex:inquirySessionDescription |
| InquiryAppearances.Sequence | ex:inquiryAssetHasInquiryAppearance/ex:inquiryAppearanceSequence |
| InquiryAppearances.WitnessName | ex:inquiryAssetHasInquiryAppearance/ex:inquiryWitnessName |
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
| SealCatagory | ex:sealAssetHasSealCategory/ex:sealCategoryName |
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
| FoiAssertedDate | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewDate |
| AccessConditionName | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:accessConditionName |
| AccessConditionCode | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:accessConditionCode |
| ClosureReviewDate | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionReviewDate |
| ClosureStartDate | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionCalculationStartDate |
| ClosurePeriod | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDuration |
| ClosureEndYear | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionEndYear |
| ClosureDescription | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDescription |
| FoiExemptions.Url | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasLegislation/ex:legislationHasUkLegislation |
| FoiExemptions.Reference | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasLegislation/ex:legislationSectionReference |
| InstrumentNumber | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionInstrumentNumber |
| InstrumentSignedDate | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionInstrumentSignatureDate |
| RetentionReconsiderDate | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionRestrictionReviewDate |
| GroundForRetentionCode | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionRestrictionHasGroundForRetention/ex:GroundForRetentionCode |
| GroundForRetentionDescription | ex:assetHasVariation/ex:variationHasSensitivityReview/ex:sensitivityReviewHasSensitivityReviewRestriction/ex:retentionRestrictionHasGroundForRetention/ex:GroundForRetentionDescription |
| Address | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasContactPoint/ex:geographicalPlaceName |
| BattalionName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasBattalionMembership/ex:battalionMembershipHasBattalion/ex:battalionName |
| BirthAddress | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasBirthAddress/ex:geographicalPlaceName |
| DateOfBirth | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personDateOfBirth |
| FamilyName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personFamilyName |
| FullName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personFullName |
| GivenName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personGivenName |
| IsVeteran | ex:assetHasVeteran exists |
| NationalRegistrationNumber | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:nationalRegistrationNumber |
| NextOfKinName | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasNextOfKinRelationship/ex:nextOfKinRelationshipHasNextOfKin/ex:personFullName |
| NextOfKinTypes | Last URI segments of (ex:assetHasPerson\|ex:assetHasVeteran)/ex:personHasNextOfKinRelationship/ex:nextOfKinRelationshipHasKinship |
| SeamanServiceNumber | (ex:assetHasPerson\|ex:assetHasVeteran)/ex:seamanServiceNumber |
| Changes.DriId | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeDriId |
| Changes.DescriptionBase64 | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeDescription |
| Changes.Timestamp | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeDateTime |
| Changes.OperatorName | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeHasOperator/ex:operatorName |
| Changes.OperatorIdentifier | ex:assetHasChange\|(ex:assetHasSensitivityReview/ex:sensitivityReviewHasChange)\|ex:variationHasChange\|(ex:variationHasSensitivityReview/ex:sensitivityReviewHasChange)/ex:changeHasOperator/ex:operatorIdentifier |
| DigitalFileCount | Count of grouped variations |
| DigitalFiles.FileName | ex:assetHasVariation/(ex:variationAlternativeName or ex:variationName) |
| DigitalFiles.SortOrder | ex:assetHasVariation/ex:variationSequence |
| DigitalFiles.RedactionSequence | ex:assetHasVariation/ex:redactedVariationSequence |
| DigitalFiles.Note | ex:assetHasVariation/ex:variationNote |
| DigitalFiles.Location | ex:assetHasVariation/ex:variationRelativeLocation |
| DigitalFiles.PhysicalConditionDescription | ex:assetHasVariation/ex:variationPhysicalConditionDescription |
| DigitalFiles.ReferenceGoogleId | ex:assetHasVariation/ex:variationReferenceGoogleId |
| DigitalFiles.ReferenceParentGoogleId | ex:assetHasVariation/ex:variationReferenceParentGoogleId |
| DigitalFiles.ScannerOperatorIdentifier | ex:assetHasVariation/ex:scannerOperatorIdentifier |
| DigitalFiles.ScannerIdentifier | ex:assetHasVariation/ex:scannerIdentifier |
| DigitalFiles.ScannerGeographicalPlace | ex:assetHasVariation/ex:scannedVariationHasScannerGeographicalPlace/ex:geographicalPlaceName |
| DigitalFiles.ScannedImageCrop | Last URI segment of ex:assetHasVariation/ex:scannedVariationHasImageCrop |
| DigitalFiles.ScannedImageDeskew | Last URI segment of ex:assetHasVariation/ex:scannedVariationHasImageDeskew |
| DigitalFiles.ScannedImageSplit | Last URI segment of ex:assetHasVariation/ex:scannedVariationHasImageSplit |
| DigitalFiles.ArchivistNotes.Note | ex:assetHasVariation/ex:variationHasDatedNote/ex:archivistNote |
| DigitalFiles.ArchivistNotes.Date | ex:assetHasVariation/ex:variationHasDatedNote/(ex:archivistNoteAt or ex:datedNoteHasDate (ex:year, ex:month, ex:day)) |
| Relationships.Relationship | Depending on existance of ex:assetRelationDescription, ex:assetRelationIdentifier, ex:assetConnectedAssetNote or ex:assetHasVariation/ex:redactedVariationSequence |
| Relationships.Reference | ex:assetRelationDescription or ex:assetRelationIdentifier or ex:assetConnectedAssetNote or ex:assetReference and ex:assetHasVariation/ex:redactedVariationSequence|

## XML

Data from the staging database: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Exporter/Sparql/ExportXml.sparql)

Every object associated with the following predicates is written to its own XML file:
- ex:assetDriXml
- ex:wo409SubsetDriXml
- ex:variationDriXml
