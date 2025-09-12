using VDS.RDF;

namespace Staging;

public static class IngestVocabulary
{
    public static readonly Uri TnaNamespace = new("http://nationalarchives.gov.uk/metadata/tna#");
    public static readonly Uri DctermsNamespace = new("http://purl.org/dc/terms/");
    public static readonly Uri TransNamespace = new("http://nationalarchives.gov.uk/dri/transcription");

    public static readonly IUriNode BatchIdentifier = new UriNode(new($"{TnaNamespace}batchIdentifier"));
    public static readonly IUriNode TdrConsignmentRef = new UriNode(new($"{TnaNamespace}tdrConsignmentRef"));
    public static readonly IUriNode RelatedMaterial = new UriNode(new($"{TnaNamespace}relatedMaterial"));
    public static readonly IUriNode LegalStatus = new UriNode(new($"{TnaNamespace}legalStatus"));
    public static readonly IUriNode HeldBy = new UriNode(new($"{TnaNamespace}heldBy"));
    public static readonly IUriNode PhysicalDescription = new UriNode(new($"{TnaNamespace}physicalDescription"));
    public static readonly IUriNode Investigation = new UriNode(new($"{TnaNamespace}investigation"));
    public static readonly IUriNode EvidenceProvidedBy = new UriNode(new($"{TnaNamespace}evidenceProvidedBy"));
    public static readonly IUriNode Session = new UriNode(new($"{TnaNamespace}session"));
    public static readonly IUriNode Session_date = new UriNode(new($"{TnaNamespace}session_date"));
    public static readonly IUriNode RestrictionOnUse = new UriNode(new($"{TnaNamespace}restrictionOnUse"));
    public static readonly IUriNode Hearing_date = new UriNode(new($"{TnaNamespace}hearing_date"));
    public static readonly IUriNode WebArchiveUrl = new UriNode(new($"{TnaNamespace}webArchiveUrl"));
    public static readonly IUriNode StartDate = new UriNode(new($"{TnaNamespace}startDate"));
    public static readonly IUriNode EndDate = new UriNode(new($"{TnaNamespace}endDate"));
    public static readonly IUriNode FormerReferenceTna = new UriNode(new($"{TnaNamespace}formerReferenceTNA"));
    public static readonly IUriNode FormerReferenceDepartment = new UriNode(new($"{TnaNamespace}formerReferenceDepartment"));
    public static readonly IUriNode Classification = new UriNode(new($"{TnaNamespace}classification"));
    public static readonly IUriNode Summary = new UriNode(new($"{TnaNamespace}summary"));
    public static readonly IUriNode InternalDepartment = new UriNode(new($"{TnaNamespace}internalDepartment"));
    public static readonly IUriNode DurationMins = new UriNode(new($"{TnaNamespace}durationMins"));
    public static readonly IUriNode FilmMaker = new UriNode(new($"{TnaNamespace}filmMaker"));
    public static readonly IUriNode FilmName = new UriNode(new($"{TnaNamespace}filmName"));
    public static readonly IUriNode Photographer = new UriNode(new($"{TnaNamespace}photographer"));
    public static readonly IUriNode FullDate = new UriNode(new($"{TnaNamespace}fullDate"));
    public static readonly IUriNode DateRange = new UriNode(new($"{TnaNamespace}dateRange"));
    public static readonly IUriNode AdministrativeBackground = new UriNode(new($"{TnaNamespace}administrativeBackground"));
    public static readonly IUriNode HasRedactedFile = new UriNode(new($"{TnaNamespace}hasRedactedFile"));
    public static readonly IUriNode AttachmentFormerReference = new UriNode(new($"{TnaNamespace}attachmentFormerReference"));
    public static readonly IUriNode SeparatedMaterial = new UriNode(new($"{TnaNamespace}separatedMaterial"));

    public static readonly IUriNode Note = new UriNode(new($"{TnaNamespace}note"));
    public static readonly IUriNode CuratedDateNote = new UriNode(new($"{TnaNamespace}curatedDateNote"));
    public static readonly IUriNode PhysicalCondition = new UriNode(new($"{TnaNamespace}physicalCondition"));
    public static readonly IUriNode GoogleId = new UriNode(new($"{TnaNamespace}googleId"));
    public static readonly IUriNode GoogleParentId = new UriNode(new($"{TnaNamespace}googleParentId"));
    public static readonly IUriNode ArchivistNote = new UriNode(new($"{TnaNamespace}archivistNote"));
    public static readonly IUriNode ArchivistNoteInfo = new UriNode(new($"{TnaNamespace}archivistNoteInfo"));
    public static readonly IUriNode ArchivistNoteDate = new UriNode(new($"{TnaNamespace}archivistNoteDate"));
    public static readonly IUriNode ScanOperator = new UriNode(new($"{TnaNamespace}scanOperator"));
    public static readonly IUriNode ScanId = new UriNode(new($"{TnaNamespace}scanId"));
    public static readonly IUriNode ScanLocation = new UriNode(new($"{TnaNamespace}scanLocation"));
    public static readonly IUriNode ImageSplit = new UriNode(new($"{TnaNamespace}imageSplit"));
    public static readonly IUriNode ImageCrop = new UriNode(new($"{TnaNamespace}imageCrop"));
    public static readonly IUriNode ImageDeskew = new UriNode(new($"{TnaNamespace}imageDeskew"));

    public static readonly IUriNode Title = new UriNode(new(DctermsNamespace, "title"));
    public static readonly IUriNode Description = new UriNode(new(DctermsNamespace, "description"));
    public static readonly IUriNode Creator = new UriNode(new(DctermsNamespace, "creator"));
    public static readonly IUriNode Language = new UriNode(new(DctermsNamespace, "language"));
    public static readonly IUriNode Rights = new UriNode(new(DctermsNamespace, "rights"));
    public static readonly IUriNode Coverage = new UriNode(new(DctermsNamespace, "coverage"));

    //Local names of predicates are constructed by concatenation with the last path segment due to the lack of end forward slash in the XML namespace declaration.
    public static readonly IUriNode PaperNumber = new UriNode(new($"{TransNamespace}paperNumber"));
    public static readonly IUriNode Counties = new UriNode(new($"{TransNamespace}counties"));
    public static readonly IUriNode StartImageNumber = new UriNode(new($"{TransNamespace}startImageNumber"));
    public static readonly IUriNode EndImageNumber = new UriNode(new($"{TransNamespace}endImageNumber"));
    public static readonly IUriNode TypeOfSeal = new UriNode(new($"{TransNamespace}typeOfSeal"));
    public static readonly IUriNode SealOwner = new UriNode(new($"{TransNamespace}sealOwner"));
    public static readonly IUriNode DateOfOriginalSeal = new UriNode(new($"{TransNamespace}dateOfOriginalSeal"));
    public static readonly IUriNode ColourOfOriginalSeal = new UriNode(new($"{TransNamespace}colourOfOriginalSeal"));
    public static readonly IUriNode Dimensions = new UriNode(new($"{TransNamespace}dimensions"));
    public static readonly IUriNode PhysicalFormat = new UriNode(new($"{TransNamespace}physicalFormat"));
    public static readonly IUriNode TransRelatedMaterial = new UriNode(new($"{TransNamespace}relatedMaterial"));
    public static readonly IUriNode AdditionalInformation = new UriNode(new($"{TransNamespace}additionalInformation"));
    public static readonly IUriNode Face = new UriNode(new($"{TransNamespace}face"));

    public static readonly IUriNode DctermsDescription = new UriNode(new(DctermsNamespace, "description")); //TODO: remove after checking data
}
