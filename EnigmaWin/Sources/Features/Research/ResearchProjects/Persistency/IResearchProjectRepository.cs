// IResearchProjectRepository.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Threading.Tasks;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;

public interface IResearchProjectRepository
{
    Task<IEnumerable<string>> FetchAllNamesAsync();
    Task<IEnumerable<ResearchProject>> FetchAllAsync();
    Task<ResearchProject?> FetchByNameAsync(string name);
    Task AddAsync(ResearchProject project);
    Task DeleteAsync(int id);
}
