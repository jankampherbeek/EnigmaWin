
// Rosetta.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EnigmaWin.Sources.Features.Localization;

public static class Rosetta
{
    
    private static string _currentLanguage = "undefined";
    private const string TextNotFound = "-- Text not found --";
    private static Dictionary<string, string> _texts = new();
    
    /// <summary>Set the preferred language.</summary>
    /// <param name="newLanguage">Indicator for the new language, either 'en' for English, 'nl' for Dutch,
    /// 'fr' for French or 'de' for German.</param>
    /// <remarks>If an invalid language is used, Enigma will not change the existing language.</remarks>
    public static void SetLanguage(string newLanguage)
    {
        if (newLanguage != "en" && newLanguage != "nl" && newLanguage != "de" && newLanguage != "fr" ) return;
        _currentLanguage = newLanguage;
        ReadTextsForLanguage();
    }

    /// <summary>Return indication for currently selected language.</summary>
    /// <returns>String for the language, either 'en', 'nl', 'de' or 'fr'.</returns>
    public static string GetLanguage()
    {
        return _currentLanguage;
    }

    /// <summary>Get text in current active language.</summary>
    /// <param name="rbKey">The key to search for in the Resource Bundle.</param>
    /// <returns>If found, the text, otherwise and indication that the text was not found.</returns>
    public static string GetText(string rbKey)
    {
        return _texts.GetValueOrDefault(rbKey, TextNotFound);
    }
    
    private static void ReadTextsForLanguage()
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var location = currentDir + @"resources\i18n\" + _currentLanguage + ".strings";
        
        List<string?> csvLines = [];
        if (!File.Exists(location))
        {
            Serilog.Log.Error("Rosetta: File {location} not found.", location);
            return;
        }
        using var reader = new StreamReader(location, Encoding.GetEncoding("UTF-8"));
        while (reader.Peek() >= 0)
        {
            csvLines.Add(reader.ReadLine());
        }
        const char splitter = '=';
        _texts = (from line in csvLines where line.Contains('=') select line.Split(splitter)).
            ToDictionary(
                keyAndValue => keyAndValue[0].Trim().Trim('"'),
                keyAndValue => keyAndValue[1].Trim().Trim('"').TrimEnd(';', '"').Trim());
    }
}