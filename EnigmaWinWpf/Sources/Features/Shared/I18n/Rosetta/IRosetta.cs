// IRosetta.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

public interface IRosetta
{
    void SetLanguage(string language);
    string GetLanguage();
    string GetText(RbFile file, string key);
}
