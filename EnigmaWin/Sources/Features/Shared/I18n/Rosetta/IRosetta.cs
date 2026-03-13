namespace EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

public interface IRosetta
{
    void SetLanguage(string language);
    string GetLanguage();
    string GetText(RbFile file, string key);
}
