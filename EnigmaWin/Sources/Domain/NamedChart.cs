using System;

namespace EnigmaWin.Sources.Domain;

public sealed record NamedChart(Guid Id, string Name, FullChart Chart);
