// ResearchProjectRepositoryTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;
using Microsoft.Data.Sqlite;
using EnigmaWintest.Features.Config;

namespace EnigmaWintest.Features.Research;

[TestFixture]
public class ResearchProjectRepositoryTests : IDisposable
{
    private InMemoryDbConnectionFactory _factory = null!;
    private ResearchProjectRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new InMemoryDbConnectionFactory();
        using var conn = _factory.Create();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS ResearchProject (
                Id               INTEGER PRIMARY KEY AUTOINCREMENT,
                Name             TEXT    NOT NULL,
                Description      TEXT    NOT NULL DEFAULT '',
                Inquiry          INTEGER NOT NULL DEFAULT 0,
                Config           TEXT    NOT NULL DEFAULT '',
                CgMultiplication INTEGER NOT NULL DEFAULT 1,
                Path             TEXT    NOT NULL DEFAULT '',
                CreationDate     TEXT    NOT NULL
            )
            """);
        _repo = new ResearchProjectRepository(_factory);
    }

    [TearDown]
    public void TearDown() => _factory.Dispose();

    public void Dispose() => _factory?.Dispose();

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ResearchProject Sample(
        string name             = "Project Alpha",
        string description      = "A test project",
        InquiriesEnum inquiry   = InquiriesEnum.FactorsInSigns,
        string config           = "{}",
        int cgMultiplication    = 1,
        string path             = "/tmp/alpha") => new()
    {
        Name             = name,
        Description      = description,
        Inquiry          = inquiry,
        Config           = config,
        CgMultiplication = cgMultiplication,
        Path             = path,
        CreationDate     = DateTime.UtcNow
    };

    // ── Add ──────────────────────────────────────────────────────────────────

    [Test]
    public async Task Add_StoresName()
    {
        await _repo.AddAsync(Sample(name: "Project Alpha"));
        var names = await _repo.FetchAllNamesAsync();
        Assert.That(names, Contains.Item("Project Alpha"));
    }

    [Test]
    public async Task Add_StoresEnquiry()
    {
        await _repo.AddAsync(Sample(inquiry: InquiriesEnum.Aspects));
        var all = await _repo.FetchAllAsync();
        Assert.That(all.Single().Inquiry, Is.EqualTo(InquiriesEnum.Aspects));
    }

    [Test]
    public async Task Add_StoresAllFields()
    {
        var date = new DateTime(2026, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var project = new ResearchProject
        {
            Name             = "Full Project",
            Description      = "Full description",
            Inquiry          = InquiriesEnum.Midpoints,
            Config           = "{\"key\":\"value\"}",
            CgMultiplication = 3,
            Path             = "/data/full",
            CreationDate     = date
        };
        await _repo.AddAsync(project);
        var fetched = await _repo.FetchByNameAsync("Full Project");

        Assert.That(fetched,                    Is.Not.Null);
        Assert.That(fetched!.Description,        Is.EqualTo("Full description"));
        Assert.That(fetched.Inquiry,             Is.EqualTo(InquiriesEnum.Midpoints));
        Assert.That(fetched.Config,              Is.EqualTo("{\"key\":\"value\"}"));
        Assert.That(fetched.CgMultiplication,    Is.EqualTo(3));
        Assert.That(fetched.Path,                Is.EqualTo("/data/full"));
        Assert.That(fetched.CreationDate,        Is.EqualTo(date));
    }

    [Test]
    public async Task Add_TwoDifferentProjects_BothStored()
    {
        await _repo.AddAsync(Sample(name: "Alpha"));
        await _repo.AddAsync(Sample(name: "Beta"));
        var all = await _repo.FetchAllAsync();
        Assert.That(all.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Add_AssignsAutoIncrementId()
    {
        await _repo.AddAsync(Sample(name: "Alpha"));
        await _repo.AddAsync(Sample(name: "Beta"));
        var all = (await _repo.FetchAllAsync()).ToList();
        Assert.That(all[0].Id, Is.Not.EqualTo(all[1].Id));
        Assert.That(all[0].Id, Is.GreaterThan(0));
    }

    // ── FetchAllNames ─────────────────────────────────────────────────────────

    [Test]
    public async Task FetchAllNames_EmptyDb_ReturnsEmpty()
    {
        var names = await _repo.FetchAllNamesAsync();
        Assert.That(names, Is.Empty);
    }

    [Test]
    public async Task FetchAllNames_ReturnsSortedAlphabetically()
    {
        await _repo.AddAsync(Sample(name: "Zeta"));
        await _repo.AddAsync(Sample(name: "Alpha"));
        await _repo.AddAsync(Sample(name: "Mu"));
        var names = (await _repo.FetchAllNamesAsync()).ToList();
        Assert.That(names, Is.EqualTo(new[] { "Alpha", "Mu", "Zeta" }));
    }

    // ── FetchAll ──────────────────────────────────────────────────────────────

    [Test]
    public async Task FetchAll_EmptyDb_ReturnsEmpty()
    {
        var all = await _repo.FetchAllAsync();
        Assert.That(all, Is.Empty);
    }

    [Test]
    public async Task FetchAll_ReturnsSortedByName()
    {
        await _repo.AddAsync(Sample(name: "Zeta"));
        await _repo.AddAsync(Sample(name: "Alpha"));
        var all = (await _repo.FetchAllAsync()).ToList();
        Assert.That(all[0].Name, Is.EqualTo("Alpha"));
        Assert.That(all[1].Name, Is.EqualTo("Zeta"));
    }

    [Test]
    public async Task FetchAll_ReturnsAllProjects()
    {
        await _repo.AddAsync(Sample(name: "Alpha"));
        await _repo.AddAsync(Sample(name: "Beta"));
        var all = await _repo.FetchAllAsync();
        Assert.That(all.Count(), Is.EqualTo(2));
    }

    // ── FetchByName ───────────────────────────────────────────────────────────

    [Test]
    public async Task FetchByName_ExistingName_ReturnsProject()
    {
        await _repo.AddAsync(Sample(name: "Alpha"));
        var result = await _repo.FetchByNameAsync("Alpha");
        Assert.That(result?.Name, Is.EqualTo("Alpha"));
    }

    [Test]
    public async Task FetchByName_NonExistentName_ReturnsNull()
    {
        await _repo.AddAsync(Sample(name: "Alpha"));
        var result = await _repo.FetchByNameAsync("Nonexistent");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task FetchByName_ReturnsCorrectProjectAmongMultiple()
    {
        await _repo.AddAsync(Sample(name: "Alpha", inquiry: InquiriesEnum.FactorsInSigns));
        await _repo.AddAsync(Sample(name: "Beta",  inquiry: InquiriesEnum.Aspects));
        var result = await _repo.FetchByNameAsync("Beta");
        Assert.That(result?.Inquiry, Is.EqualTo(InquiriesEnum.Aspects));
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Delete_RemovesProject()
    {
        await _repo.AddAsync(Sample(name: "Alpha"));
        var project = (await _repo.FetchAllAsync()).Single();
        await _repo.DeleteAsync(project.Id);
        Assert.That(await _repo.FetchAllAsync(), Is.Empty);
    }

    [Test]
    public async Task Delete_RemovesOnlyTargetProject()
    {
        await _repo.AddAsync(Sample(name: "Alpha"));
        await _repo.AddAsync(Sample(name: "Beta"));
        var all    = (await _repo.FetchAllAsync()).ToList();
        var alpha  = all.First(p => p.Name == "Alpha");
        await _repo.DeleteAsync(alpha.Id);
        var remaining = (await _repo.FetchAllAsync()).ToList();
        Assert.That(remaining.Count,        Is.EqualTo(1));
        Assert.That(remaining[0].Name,      Is.EqualTo("Beta"));
    }

    [Test]
    public async Task Delete_NonExistentId_DoesNotThrow()
    {
        Assert.DoesNotThrowAsync(() => _repo.DeleteAsync(9999));
    }

    // ── Inquiry round-trip ────────────────────────────────────────────────────

    [Test]
    public async Task Inquiry_RoundTrip_AllValues()
    {
        foreach (var inquiry in Enum.GetValues<InquiriesEnum>())
        {
            await _repo.AddAsync(Sample(name: inquiry.ToString(), inquiry: inquiry));
            var fetched = await _repo.FetchByNameAsync(inquiry.ToString());
            Assert.That(fetched?.Inquiry, Is.EqualTo(inquiry), $"Round-trip failed for {inquiry}");
            var all = (await _repo.FetchAllAsync()).ToList();
            await _repo.DeleteAsync(all.First(p => p.Name == inquiry.ToString()).Id);
        }
    }
}
