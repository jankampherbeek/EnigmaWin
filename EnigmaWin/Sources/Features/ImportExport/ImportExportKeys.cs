// ImportExportKeys.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.ImportExport;

/// <summary>Localization keys for the Import/Export feature, resolved from ImportExport.strings.</summary>
public static class ImportExportKeys
{
    // ── Workspace ────────────────────────────────────────────────────────────
    public const string Title  = "view.importexport.title";
    public const string Select = "view.importexport.select";

    // ── Format screens ───────────────────────────────────────────────────────
    public const string ExportButton        = "view.importexport.export.button";
    public const string ImportButton        = "view.importexport.import.button";
    public const string RecordMessage       = "view.importexport.record.message";
    public const string ErrorReadFile       = "view.importexport.error.readfile";
    public const string ErrorWriteFile      = "view.importexport.error.writefile";
    public const string ErrorDatabase       = "view.importexport.error.database";
    public const string FilterAllFiles      = "view.importexport.filter.allfiles";

    public const string EnigmaTitle         = "view.importexport.enigma.title";
    public const string EnigmaBody          = "view.importexport.enigma.body";
    public const string EnigmaFilter        = "view.importexport.enigma.filter";
    public const string EnigmaExportSuccess = "view.importexport.enigma.export.success";
    public const string EnigmaImportSuccess = "view.importexport.enigma.import.success";

    public const string QckTitle            = "view.importexport.qck.title";
    public const string QckBody             = "view.importexport.qck.body";
    public const string QckFilter           = "view.importexport.qck.filter";
    public const string QckExportSuccess    = "view.importexport.qck.export.success";
    public const string QckImportSuccess    = "view.importexport.qck.import.success";

    public const string AafTitle            = "view.importexport.aaf.title";
    public const string AafBody             = "view.importexport.aaf.body";
    public const string AafFilter           = "view.importexport.aaf.filter";
    public const string AafExportSuccess    = "view.importexport.aaf.export.success";
    public const string AafImportSuccess    = "view.importexport.aaf.import.success";

    // ── Diagnostics: shared ──────────────────────────────────────────────────
    public const string MsgEncodingUnconfirmed = "importexport.msg.encoding.unconfirmed";
    public const string MsgEncodingNotUtf8     = "importexport.msg.encoding.notutf8";
    public const string MsgEncodingLoss        = "importexport.msg.encoding.loss";
    public const string MsgInvalidDate         = "importexport.msg.invaliddate";
    public const string MsgNoDateTime          = "importexport.msg.nodatetime";
    public const string MsgNoCoordinates       = "importexport.msg.nocoordinates";
    public const string MsgSaveFailed          = "importexport.msg.savefailed";

    // ── Diagnostics: QCK ─────────────────────────────────────────────────────
    public const string MsgQckInvalidLength      = "importexport.msg.qck.invalidlength";
    public const string MsgQckInvalidMonth       = "importexport.msg.qck.invalidmonth";
    public const string MsgQckInvalidNumber      = "importexport.msg.qck.invalidnumber";
    public const string MsgQckYearRange          = "importexport.msg.qck.yearrange";
    public const string MsgQckBeforeCutover      = "importexport.msg.qck.beforecutover";
    public const string MsgQckInvalidTime        = "importexport.msg.qck.invalidtime";
    public const string MsgQckInvalidAmPm        = "importexport.msg.qck.invalidampm";
    public const string MsgQckTimeRange          = "importexport.msg.qck.timerange";
    public const string MsgQckNoonBug            = "importexport.msg.qck.noonbug";
    public const string MsgQckInvalidCorrection  = "importexport.msg.qck.invalidcorrection";
    public const string MsgQckInvalidLongitude   = "importexport.msg.qck.invalidlongitude";
    public const string MsgQckLongitudeDirection = "importexport.msg.qck.longitudedirection";
    public const string MsgQckLongitudeRange     = "importexport.msg.qck.longituderange";
    public const string MsgQckInvalidLatitude    = "importexport.msg.qck.invalidlatitude";
    public const string MsgQckLatitudeDirection  = "importexport.msg.qck.latitudedirection";
    public const string MsgQckLatitudeRange      = "importexport.msg.qck.latituderange";
    public const string MsgQckLmtMismatch        = "importexport.msg.qck.lmtmismatch";
    public const string MsgQckTzAbbreviation     = "importexport.msg.qck.tzabbreviation";
    public const string MsgQckTzNotPreserved     = "importexport.msg.qck.tznotpreserved";
    public const string MsgQckWriteYearRange     = "importexport.msg.qck.writeyearrange";
    public const string MsgQckWriteMonth         = "importexport.msg.qck.writemonth";
    public const string MsgQckTruncated          = "importexport.msg.qck.truncated";

    // ── Diagnostics: AAF'97 ──────────────────────────────────────────────────
    public const string MsgAafChunkBeforeA93          = "importexport.msg.aaf.chunkbeforea93";
    public const string MsgAafUnknownChunk            = "importexport.msg.aaf.unknownchunk";
    public const string MsgAafA93FieldCount           = "importexport.msg.aaf.a93fieldcount";
    public const string MsgAafNoB93                   = "importexport.msg.aaf.nob93";
    public const string MsgAafB93FieldCount           = "importexport.msg.aaf.b93fieldcount";
    public const string MsgAafInvalidEnid             = "importexport.msg.aaf.invalidenid";
    public const string MsgAafInvalidDate             = "importexport.msg.aaf.invaliddate";
    public const string MsgAafInvalidYear             = "importexport.msg.aaf.invalidyear";
    public const string MsgAafInvalidTime             = "importexport.msg.aaf.invalidtime";
    public const string MsgAafTimeRange               = "importexport.msg.aaf.timerange";
    public const string MsgAafInvalidCoordinate       = "importexport.msg.aaf.invalidcoordinate";
    public const string MsgAafCoordinateRange         = "importexport.msg.aaf.coordinaterange";
    public const string MsgAafInvalidOffset           = "importexport.msg.aaf.invalidoffset";
    public const string MsgAafInvalidOffsetDirection  = "importexport.msg.aaf.invalidoffsetdirection";
    public const string MsgAafJdMismatch              = "importexport.msg.aaf.jdmismatch";
    public const string MsgAafGender                  = "importexport.msg.aaf.gender";
    public const string MsgAafUnknownOffset           = "importexport.msg.aaf.unknownoffset";
    public const string MsgAafUnknownTimeType         = "importexport.msg.aaf.unknowntimetype";
    public const string MsgAafTzNotPreserved          = "importexport.msg.aaf.tznotpreserved";
    public const string MsgAafPlaceComma              = "importexport.msg.aaf.placecomma";
    public const string MsgAafFieldComma              = "importexport.msg.aaf.fieldcomma";

    // ── Diagnostics: Enigma JSON ─────────────────────────────────────────────
    public const string MsgEnigmaInvalidFile        = "importexport.msg.enigma.invalidfile";
    public const string MsgEnigmaUnsupportedVersion = "importexport.msg.enigma.unsupportedversion";
    public const string MsgEnigmaEventNoCharts      = "importexport.msg.enigma.eventnocharts";
}
