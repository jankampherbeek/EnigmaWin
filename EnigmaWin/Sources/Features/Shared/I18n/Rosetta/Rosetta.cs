// Rosetta.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

public sealed class Rosetta : IRosetta
{
    private string _currentLanguage = "undefined";
    private const string TextNotFound = "-- Text not found --";
    private readonly Dictionary<RbFile, Dictionary<string, string>> _textsByFile = new();

    /// <summary>Set the preferred language and load all resource files for that language.</summary>
    /// <param name="language">'en', 'nl', 'de' or 'fr'.</param>
    public void SetLanguage(string language)
    {
        if (language != "en" && language != "nl" && language != "de" && language != "fr") return;
        _currentLanguage = language;
        LoadAllFiles();
    }

    public string GetLanguage() => _currentLanguage;

    /// <summary>Get text from the specified resource bundle file in the current language.</summary>
    /// <param name="file">The resource bundle file to search in.</param>
    /// <param name="key">The key to look up.</param>
    public string GetText(RbFile file, string key)
    {
        return _textsByFile.TryGetValue(file, out var texts)
            ? texts.GetValueOrDefault(key, TextNotFound)
            : TextNotFound;
    }

    private void LoadAllFiles()
    {
        _textsByFile.Clear();
        foreach (var file in Enum.GetValues<RbFile>())
            _textsByFile[file] = LoadFile(file);
    }

    private Dictionary<string, string> LoadFile(RbFile file)
    {
        var location = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources", "i18n", _currentLanguage, $"{file}.strings");

        if (!File.Exists(location))
        {
            Serilog.Log.Warning("Rosetta: File {Location} not found.", location);
            return new Dictionary<string, string>();
        }

        var lines = new List<string>();
        using var reader = new StreamReader(location, Encoding.UTF8);
        while (reader.Peek() >= 0)
        {
            var line = reader.ReadLine();
            if (line != null) lines.Add(line);
        }

        const char splitter = '=';
        return lines
            .Where(line => line.Contains(splitter) && !line.TrimStart().StartsWith("//") && !line.TrimStart().StartsWith("/*"))
            .Select(line => line.Split(splitter, 2))
            .ToDictionary(
                parts => parts[0].Trim().Trim('"'),
                parts => parts[1].Trim().Trim('"').TrimEnd(';', '"').Trim());
    }
}
