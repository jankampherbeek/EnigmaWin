// ResearchProject.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;

namespace EnigmaWin.Sources.Domain;

public sealed record ResearchProject
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public InquiriesEnum Inquiry { get; init; } = InquiriesEnum.FactorsInSigns;
    public string Config { get; init; } = "";
    public int CgMultiplication { get; init; }
    public string Path { get; init; } = "";
    public DateTime CreationDate { get; init; } = DateTime.UtcNow;
}
