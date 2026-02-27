# Migration mapping

## Namespaces

- dri: [http://nationalarchives.gov.uk/terms/dri#](https://github.com/digital-preservation/dri-vocabulary/blob/master/terms/dri_vocabulary.ttl)
- ex: [http://id.example.com/schema/](https://github.com/nationalarchives/dri-data-migration/blob/main/Ontology.ttl)
- dcterms: http://purl.org/dc/terms/
- rdfs: http://www.w3.org/2000/01/rdf-schema#
- prov: http://www.w3.org/ns/prov#
- tna: http://nationalarchives.gov.uk/metadata/tna#
- tnas: http://nationalarchives.gov.uk/metadata/spatial/
- tnap: http://nationalarchives.gov.uk/metadata/person/
- trans: http://nationalarchives.gov.uk/dri/transcription

## Access condition

### Sequence step 1

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetAccessCondition.sparql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/AccessConditionGraph.sparql)

| Source | Target |
| --- | --- |
| dri:ClosureType (subject's fragment) | ex:accessConditionCode |
| dri:ClosureType (rdfs:label) | ex:accessConditionName |

## Legislation

### Sequence step 2

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetLegislation.sparql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/LegislationGraph.sparql)

| Source | Target |
| --- | --- |
| dri:exemptionCode/rdfs:label (object's fragment) | ex:legislationSectionReference |
| dri:exemptionCode | ex:legislationHasUkLegislation |

## Ground for retention

### Sequence step 3

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetGroundForRetention.sparql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/GroundForRetentionGraph.sparql)

| Source | Target |
| --- | --- |
| dri:RetentionJustificationType (rdfs:label) | ex:groundForRetentionCode |
| dri:RetentionJustificationType (rdfs:comment) | ex:groundForRetentionDescription |

## Subset

### Sequence step 4

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetSubset.sparql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/SubsetGraph.sparql)

| Source | Target |
| --- | --- |
| dri:DeliverableUnit (rdfs:label) | ex:subsetReference |
| dri:DeliverableUnit (dri:parent/rdfs:label) or dri:DeliverableUnit (dri:hasDirectory/rdfs:label) or series code | ex:subsetHasBroaderSubset/ex:subsetReference |
| dri:DeliverableUnit (dri:hasDirectory/rdfs:label) | ex:subsetHasRetention/ex:importLocation |

### Sequence step 5

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetAdm158SubsetDeliverableUnit.sql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/Adm158SubsetDeliverableUnitGraph.sparql)

| Source | Target |
| --- | --- |
| _SQLite_ | |
| deliverableunit.CATALOGUEREFERENCE | ex:subsetReference |
| xmlmetadata.XMLCLOB | ex:adm158SubsetDriXml |
| _XMLCLOB_ | |
| trans:surname | ex:subsetHasPerson/ex:personFamilyName |
| trans:surnameOther | ex:subsetHasPerson/ex:personAlternativeFamilyName |
| trans:forenames | ex:subsetHasPerson/ex:personGivenName |
| trans:forenamesOther | ex:subsetHasPerson/ex:personAlternativeGivenName |
| trans:ageYears && trans:ageMonths | ex:subsetHasPerson/ex:personAge |
| trans:placeOfBirthParish | ex:subsetHasPerson/ex:personHasBirthAddress/ex:parish |
| trans:placeOfBirthTown | ex:subsetHasPerson/ex:personHasBirthAddress/ex:town |
| trans:placeOfBirthCounty | ex:subsetHasPerson/ex:personHasBirthAddress/ex:county |
| trans:placeOfBirthCountry | ex:subsetHasPerson/ex:personHasBirthAddress/ex:country |
| trans:divisionDescription | ex:subsetHasPerson/ex:personHasNavyMembership/ex:navyMembershipHasNavyDivision/ex:navyDivisionName |

## Asset

### Sequence step 6

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetAsset.sparql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/AssetGraph.sparql)

| Source | Target |
| --- | --- |
| dri:DeliverableUnit (subject's last URI segment) | ex:assetDriId |
| dri:DeliverableUnit (rdfs:label) | ex:assetReference |
| dri:DeliverableUnit (dri:parent/rdfs:label) or series code | ex:assetHasSubset/ex:subsetReference |
| dri:DeliverableUnit (dri:hasDirectory/rdfs:label or dri:parent/dri:hasDirectory/rdfs:label) | ex:assetHasRetention/ex:importLocation |
| dri:DeliverableUnit (dri:isPartOfUnit where matches dri:Batch rdfs:label) | ex:assetHasTransfer/ex:transferHasFormalBody/ex:formalBodyName |
| dri:DeliverableUnit (dri:isPartOfSeries/dri:creatingBody) | ex:assetHasCreation/ex:creationHasFormalBody/ex:formalBodyName |

### Sequence step 8

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetAssetDeliverableUnit.sql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/AssetDeliverableUnitGraph.sparql)

| Source | Target |
| --- | --- |
| _SQLite_ | |
| deliverableunit.SECURITYTAG | ex:assetHasAssetTagType |
| xmlmetadata.XMLCLOB | ex:assetDriXml |
| _XMLCLOB_ | |
| tna:batchIdentifier | ex:batchDriId |
| tna:tdrConsignmentRef | ex:consignmentTdrReference |
| tna:tdrFileReference | ex:fileTdrReference |
| tna:tdrParentReference | ex:parentTdrReference |
| tna:tdrUuid | ex:assetTdrId |
| dcterms:description or tna:contentManagementSystemContainer or tna:summary or tna:additionalInformation or tna:itemDescription | ex:assetDescription |
| tna:administrativeBackground | ex:assetSummary |
| tna:relatedMaterial or trans:relatedMaterial | ex:assetRelationDescription |
| tna:relatedIaid | ex:assetRelationIdentifier and ex:assetRelationReference |
| tna:physicalDescription or trans:physicalFormat | ex:assetPhysicalDescription |
| tna:evidenceProvidedBy | ex:inquiryAssetHasEvidenceProvider/ex:inquiryEvidenceProviderName |
| tna:investigation | ex:inquiryAssetHasInquiryInvestigation/ex:inquiryInvestigationName |
| tna:restrictionOnUse | ex:assetUsageRestrictionDescription |
| tna:formerReferenceTNA | ex:assetPastReference |
| tna:formerReferenceDepartment | ex:assetPreviousReference |
| tna:classification | ex:assetTag |
| tna:internalDepartment | ex:assetSourceInternalName |
| tna:filmMaker | ex:filmProductionCompanyName |
| tna:filmName | ex:filmTitle |
| tna:photographer | ex:photographerDescription |
| trans:paperNumber | ex:paperNumber |
| trans:poorLawUnionNumber | ex:poorLawUnionNumber |
| trans:sealOwner | ex:sealOwnerName |
| trans:colourOfOriginalSeal | ex:sealColour |
| trans:separatedMaterial | ex:assetConnectedAssetNote |
| dcterms:modified | ex:assetModifiedAt |
| tna:curatedDate | ex:assetAlternativeModifiedAt or ex:assetHasAlternativeModifiedDateStart and ex:assetHasAlternativeModifiedDateEnd |
| tna:curatedDateNote | ex:assetAlternativeModifiedAtNote |
| tna:attachmentFormerReference | ex:emailAttachmentReference |
| tna:session_date | ex:courtSessionDate |
| tna:hearing_date | ex:inquiryHearingDate |
| trans:startImageNumber | ex:imageSequenceStart |
| trans:endImageNumber | ex:imageSequenceEnd |
| dcterms:title | ex:assetName or ex:assetAlternativeName |
| tna:hasRedactedFile | ex:assetHasVariation and ex:redactedVariationSequence |
| tna:hasPresentationManifestationFile | ex:assetHasVariation and ex:presentationVariationSequence |
| tna:durationMins | ex:filmDuration |
| tna:webArchiveUrl | ex:assetHasUkGovernmentWebArchive |
| {Sequence} of the case | ex:courtAssetHasCourtCase/ex:courtCaseSequence |
| tna:case_id_{Sequence} | ex:courtAssetHasCourtCase/ex:courtCaseReference |
| tna:case_name_{Sequence} | ex:courtAssetHasCourtCase/ex:courtCaseName |
| tna:case_summary_{Sequence} | ex:courtAssetHasCourtCase/ex:courtCaseSummary |
| tna:case_summary_{Sequence}_judgment | ex:courtAssetHasCourtCase/ex:courtCaseSummaryJudgment |
| tna:case_summary_{Sequence}_reasons_for_judgment | ex:courtAssetHasCourtCase/ex:courtCaseSummaryReasonsForJudgment |
| tna:hearing_start_date_{Sequence} | ex:courtAssetHasCourtCase/ex:courtCaseHearingStartDate |
| tna:hearing_end_date_{Sequence} | ex:courtAssetHasCourtCase/ex:courtCaseHearingEndDate |
| {Sequence} of the inquiry appearance | ex:inquiryAssetHasInquiryAppearance/ex:inquiryAppearanceSequence |
| tna:witness_list_{Sequence} | ex:inquiryAssetHasInquiryAppearance/ex:inquiryAppearanceHasInquiryWitness/ex:inquiryWitnessName |
| tna:subject_role_{Sequence} | ex:inquiryAssetHasInquiryAppearance/ex:inquiryWitnessAppearanceDescription |
| tna:session | ex:inquirySessionDescription |
| dcterms:coverage/tna:startDate or dcterms:coverage/tna:fullDate or dcterms:coverage/tna:dateRange | ex:assetHasOriginDateStart or ex:assetHasOriginApproximateDateStart |
| dcterms:coverage/tna:endDate or dcterms:coverage/tna:fullDate or dcterms:coverage/tna:dateRange | ex:assetHasOriginDateEnd or ex:assetHasOriginApproximateDateEnd |
| dcterms:language | ex:assetHasLanguage/ex:languageName |
| trans:counties or tnas:county | ex:assetHasAssociatedGeographicalPlace/ex:geographicalPlaceName |
| tna:heldBy | ex:assetHasRetention/ex:retentionHasFormalBody/ex:formalBodyName |
| dcterms:rights | ex:assetHasCopyright/ex:copyrightTitle |
| tna:legalStatus | ex:assetHasLegalStatus |
| trans:typeOfSeal | ex:sealAssetHasSealCategory/ex:sealCategoryName |
| trans:dateOfOriginalSeal | ex:sealAssetHasStartDate or ex:sealAssetHasObverseStartDate or ex:sealAssetHasReverseStartDate or ex:sealAssetHasEndDate or ex:sealAssetHasObverseEndDate or ex:sealAssetHasReverseEndDate |
| trans:dimensions | ex:dimensionVerbatim and ex:measurableAssetHasDimension or ex:sealAssetHasObverseDimension or ex:sealAssetHasReverseDimension or {any of the previous}/(ex:firstDimensionMillimetre\|ex:secondDimensionMillimetre) |
| trans:surname | ex:assetHasPerson/ex:personFamilyName |
| trans:forenames | ex:assetHasPerson/ex:personGivenName |
| trans:officialNumber | ex:assetHasPerson/ex:seamanServiceNumber |
| trans:birthDate/trans:date | ex:assetHasPerson/ex:personHasDateOfBirth |
| trans:placeOfBirth | ex:personHasBirthAddress/ex:geographicalPlaceName |

### Sequence step 9

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetWo409SubsetDeliverableUnit.sql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/Wo409SubsetDeliverableUnitGraph.sparql)

| Source | Target |
| --- | --- |
| _SQLite_ | |
| deliverableunit.DELIVERABLEUNITREF | ex:wo409SubsetDriId |
| xmlmetadata.XMLCLOB | ex:wo409SubsetDriXml |
| _XMLCLOB_ | |
| tnap:namePart | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personFullName or (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personGivenName or (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personFamilyName |
| tnap:nationalRegistrationNumber | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:nationalRegistrationNumber |
| dcterms:subject/tnas:address/tnas:addressString | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personHasContactPoint/ex:geographicalPlaceName |
| dcterms:subject/tnap:birth/tnap:date | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personHasDateOfBirth |
| dcterms:subject/tnas:birth/tnas:addressString | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personHasBirthAddress/ex:geographicalPlaceName |
| tnas:county and dcterms:references | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personHasBattalionMembership/ex:battalionMembershipHasBattalion/ex:battalionName |
| tnap:relation/tnap:person/tnap:name/tnap:nameString | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personHasNextOfKinRelationship/ex:nextOfKinRelationshipHasNextOfKin/ex:personFullName |
| tnap:relation (malformed RDF) | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personHasNextOfKinRelationship/ex:nextOfKinRelationshipHasKinship |
| type (malformed RDF) | (ex:assetHasVeteran\|ex:assetHasPerson)/ex:personHasNextOfKinRelationship/ex:kinshipVerbatim |

## Variation

### Sequence step 7

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetVariation.sparql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/VariationGraph.sparql)

| Source | Target |
| --- | --- |
| dri:File (subject's last URI segment) | ex:variationDriId |
| dri:File (rdfs:label) | ex:variationName |
| dri:File (^dri:file/dri:parent/rdfs:label) | ex:variationHasAsset/ex:assetReference |

### Sequence step 10

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetVariationFile.sql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/VariationFileGraph.sparql)

| Source | Target |
| --- | --- |
| _SQLite_ | |
| digitalfile.FILELOCATION + '/' + digitalfile.NAME | ex:variationRelativeLocation |
| deliverableunitmanifestation.MANIFESTATIONREF | ex:variationDriManifestationId |
| digitalfile.FILESIZE | ex:variationSizeBytes |
| digitalfilefixityinfo.ALGORITHMNAME | ex:variationHasVariationDataIntegrityCalculation/ex:variationDataIntegrityCalculationHasHashFunction |
| digitalfilefixityinfo.FIXITYVALUE | ex:variationHasVariationDataIntegrityCalculation/ex:checksum |
| xmlmetadata.XMLCLOB | ex:variationDriXml |
| _XMLCLOB_ | |
| rdfs:comment or tna:note | ex:variationNote |
| tna:physicalCondition | ex:variationPhysicalConditionDescription |
| tna:googleId | ex:variationReferenceGoogleId |
| tna:googleParentId | ex:variationReferenceParentGoogleId |
| tna:scanId | ex:scannerIdentifier |
| tna:scanOperator | ex:scannerOperatorIdentifier |
| tna:ordinal | ex:variationSequence |
| tna:scanLocation | ex:scannedVariationHasScannerGeographicalPlace/ex:geographicalPlaceName |
| tna:imageSplit | ex:scannedVariationHasImageSplit |
| tna:imageCrop | ex:scannedVariationHasImageCrop |
| tna:imageDeskew | ex:scannedVariationHasImageDeskew |
| tna:curatedTitle | ex:variationAlternativeName |
| tna:archivistNote/tna:archivistNoteInfo | ex:variationHasDatedNote/ex:archivistNote |
| tna:archivistNote/tna:archivistNoteDate | ex:variationHasDatedNote/ex:datedNoteHasDate |


## Sensitivity review

### Sequence step 11

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sparql/GetSensitivityReview.sparql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/SensitivityReviewGraph.sparql)

| Source | Target |
| --- | --- |
| prov:specializationOf (subject's last URI segment) | ex:sensitivityReviewDriId |
| prov:specializationOf/rdfs:label | ex:sensitivityReviewHasSubset/ex:subsetReference or ex:SensitivityReviewHasAsset/ex:assetReference |
| prov:specializationOf (object's last URI segment) | ex:sensitivityReviewHasVariation/ex:variationDriId |
| dri:closureType | ex:sensitivityReviewHasAccessCondition/ex:accessConditionCode |
| dri:exemptionAsserted | ex:sensitivityReviewDate |
| dri:titleAlternative | ex:sensitivityReviewSensitiveName |
| dcterms:alternative | ex:sensitivityReviewSensitiveDescription |
| prov:wasRevisionOf | ex:sensitivityReviewHasPastSensitivityReview |
| dri:reviewDate | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionReviewDate |
| dri:startDate | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionCalculationStartDate |
| dri:closurePeriod | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDuration or ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionEndYear |
| prov:qualifiedInfluence/prov:influencer/rdfs:label | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionDescription |
| dr:exemptionCode | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasLegislation/ex:legislationHasUkLegislation |
| dri:rINumber | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasRetentionRestriction/ex:retentionInstrumentNumber |
| dri:retentionReconsiderDate | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasRetentionRestriction/ex:retentionInstrumentSignatureDate |
| dri:rISignedDate | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasRetentionRestriction/ex:retentionRestrictionReviewDate  |
| dri:retentionJustification | ex:sensitivityReviewHasSensitivityReviewRestriction/ex:sensitivityReviewRestrictionHasRetentionRestriction/ex:retentionRestrictionHasGroundForRetention/ex:groundForRetentionCode |
| prov:qualifiedGeneration (object's last URI segment) | ex:sensitivityReviewHasChange/ex:changeDriId |
| prov:qualifiedGeneration/prov:activity/rdfs:label | ex:sensitivityReviewHasChange/ex:changeDescription |
| prov:qualifiedGeneration/prov:atTime | ex:sensitivityReviewHasChange/ex:changeDateTime |
| prov:qualifiedGeneration/prov:wasAssociatedWith/rdfs:label | ex:sensitivityReviewHasChange/ex:changeHasOperator/ex:operatorName |

## Change

### Sequence step 12

Source: [query](https://github.com/nationalarchives/dri-data-migration/blob/main/Dri/Sql/GetChange.sql)\
Target: [graph](https://github.com/nationalarchives/dri-data-migration/blob/main/Staging/Sparql/ChangeGraph.sparql)

| Source | Target |
| --- | --- |
| auditchange.CHANGEREF | ex:changeDriId |
| auditchange.XMLDIFF | ex:changeDescription |
| auditchange.DATETIME | ex:changeDateTime |
| tableinvolved.TABLENAME and auditchange.PRIMARYKEYVALUE | ex:changeHasAsset or ex:changeHasVariation |
| auditchange.USERNAME | ex:changeHasOperator/ex:operatorIdentifier |
| auditchange.FULLNAME | ex:changeHasOperator/ex:operatorName |
