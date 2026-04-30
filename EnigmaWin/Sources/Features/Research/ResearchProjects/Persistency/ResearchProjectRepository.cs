// ResearchProjectRepository.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Domain;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;

namespace EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;

internal sealed class ResearchProjectRepository(IDbConnectionFactory factory) : IResearchProjectRepository
{
    public async Task<IEnumerable<string>> FetchAllNamesAsync()
    {
        using var conn = factory.Create();
        return await conn.QueryAsync<string>(
            "SELECT Name FROM ResearchProject ORDER BY Name");
    }

    public async Task<IEnumerable<ResearchProject>> FetchAllAsync()
    {
        using var conn = factory.Create();
        var rows = await conn.QueryAsync<ResearchProjectRow>(
            "SELECT * FROM ResearchProject ORDER BY Name");
        return rows.Select(ToRecord);
    }

    public ResearchProject? FetchById(int id)
    {
        using var conn = factory.Create();
        var row = conn.QuerySingleOrDefault<ResearchProjectRow>(
            "SELECT * FROM ResearchProject WHERE Id = @Id", new { Id = id });
        return row is null ? null : ToRecord(row);
    }

    public async Task<ResearchProject?> FetchByNameAsync(string name)
    {
        using var conn = factory.Create();
        var row = await conn.QuerySingleOrDefaultAsync<ResearchProjectRow>(
            "SELECT * FROM ResearchProject WHERE Name = @Name", new { Name = name });
        return row is null ? null : ToRecord(row);
    }

    public async Task AddAsync(ResearchProject project)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("""
            INSERT INTO ResearchProject (Name, Description, Inquiry, Config, CgMultiplication, Path, CreationDate)
            VALUES (@Name, @Description, @Inquiry, @Config, @CgMultiplication, @Path, @CreationDate)
            """,
            new
            {
                project.Name,
                project.Description,
                Inquiry = (int)project.Inquiry,
                project.Config,
                project.CgMultiplication,
                project.Path,
                CreationDate = project.CreationDate.ToString("O")
            });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync(
            "DELETE FROM ResearchProject WHERE Id = @Id", new { Id = id });
    }

    private static ResearchProject ToRecord(ResearchProjectRow row) => new()
    {
        Id             = row.Id,
        Name           = row.Name,
        Description    = row.Description,
        Inquiry        = (InquiriesEnum)row.Inquiry,
        Config         = row.Config,
        CgMultiplication = row.CgMultiplication,
        Path           = row.Path,
        CreationDate   = System.DateTime.Parse(row.CreationDate, null, System.Globalization.DateTimeStyles.RoundtripKind)
    };

    private sealed class ResearchProjectRow
    {
        public int    Id               { get; init; }
        public string Name             { get; init; } = "";
        public string Description      { get; init; } = "";
        public int    Inquiry          { get; init; }
        public string Config           { get; init; } = "";
        public int    CgMultiplication { get; init; }
        public string Path             { get; init; } = "";
        public string CreationDate     { get; init; } = "";
    }
}
