// LegacyTextEncoding.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnigmaWin.Sources.Features.ImportExport.Shared;

/// <summary>Decoded text of an imported file plus any encoding warnings.</summary>
public sealed record DecodedFile(string Content, IReadOnlyList<ExchangeMessage> Messages);

/// <summary>
/// Several exchange formats (QCK, AAF'97) predate any modern Unicode standard, so the encoding is never a
/// hidden platform default. Import uses Windows-1252 as the legacy encoding but accepts files that were
/// actually saved as UTF-8. Export writes Windows-1252 for QCK; AAF'97 always exports as UTF-8.
/// </summary>
public static class LegacyTextEncoding
{
    private const string LegacyEncodingName = "Windows-1252";

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static Encoding LegacyEncoding
    {
        get
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(1252, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
        }
    }

    /// <summary>
    /// Decodes a fixed-width format (QCK). Pure ASCII is identical under UTF-8 and the legacy encoding and is
    /// used without a warning. Anything else is decoded with the legacy encoding and reported, because the true
    /// encoding cannot be confirmed and multi-byte UTF-8 would shift every fixed column.
    /// </summary>
    public static DecodedFile Decode(byte[] data)
    {
        var bytes = StripUtf8Bom(data);
        if (bytes.All(b => b < 0x80))
            return new DecodedFile(Encoding.ASCII.GetString(bytes), []);

        return new DecodedFile(LegacyEncoding.GetString(bytes),
            [ExchangeMessage.Warning(ImportExportKeys.MsgEncodingUnconfirmed, null, null, LegacyEncodingName)]);
    }

    /// <summary>
    /// Decodes a format that is not fixed-width (AAF'97): full UTF-8 (accented characters included) is tried
    /// first, the legacy encoding is the fallback.
    /// </summary>
    public static DecodedFile DecodePreferringUtf8(byte[] data)
    {
        var bytes = StripUtf8Bom(data);
        try
        {
            return new DecodedFile(StrictUtf8.GetString(bytes), []);
        }
        catch (DecoderFallbackException)
        {
            return new DecodedFile(LegacyEncoding.GetString(bytes),
                [ExchangeMessage.Warning(ImportExportKeys.MsgEncodingNotUtf8, null, null, LegacyEncodingName)]);
        }
    }

    /// <summary>
    /// Encodes text with the legacy encoding. Characters that cannot be represented are replaced;
    /// <c>HadLoss</c> reports whether that happened.
    /// </summary>
    public static (byte[] Data, bool HadLoss) Encode(string text)
    {
        var data = LegacyEncoding.GetBytes(text);
        var hadLoss = LegacyEncoding.GetString(data) != text;
        return (data, hadLoss);
    }

    private static byte[] StripUtf8Bom(byte[] data) =>
        data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF ? data[3..] : data;
}
