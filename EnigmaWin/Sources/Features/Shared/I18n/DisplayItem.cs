namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Wraps an enum value with its localized display text for use in UI pickers.</summary>
public sealed record DisplayItem<T>(T Value, string DisplayText)
{
    public override string ToString() => DisplayText;
}
