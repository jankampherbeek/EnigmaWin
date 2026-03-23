// UserConfigurationRepositoryTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using System;
using System.Threading.Tasks;
using Dapper;
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using Microsoft.Data.Sqlite;

namespace EnigmaWintest.Features.Config;

/// <summary>
/// Integration tests for UserConfigurationRepository using an in-memory SQLite database.
/// </summary>
[TestFixture]
public class UserConfigurationRepositoryTests : IDisposable
{
    private InMemoryDbConnectionFactory _factory = null!;
    private UserConfigurationRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new InMemoryDbConnectionFactory();
        using var conn = _factory.Create();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS UserConfiguration (
                Id                    TEXT PRIMARY KEY,
                Name                  TEXT NOT NULL,
                IsActive              INTEGER NOT NULL DEFAULT 0,
                Language              TEXT NOT NULL DEFAULT '',
                CalculationConfigJson TEXT NOT NULL,
                DisplayConfigJson     TEXT NOT NULL,
                GlyphsConfigJson      TEXT NOT NULL,
                FactorConfigJson      TEXT NOT NULL,
                AspectConfigJson      TEXT NOT NULL,
                OrbConfigJson         TEXT NOT NULL,
                ProgressionsConfigJson TEXT NOT NULL
            )
            """);
        _repo = new UserConfigurationRepository(_factory);
    }

    [TearDown]
    public void TearDown() => _factory.Dispose();

    public void Dispose() => _factory?.Dispose();

    // ── Add ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Add_FirstConfig_IsSetActive()
    {
        var config = await _repo.AddAsync("First");
        Assert.That(config.IsActive, Is.True);
    }

    [Test]
    public async Task Add_SecondConfig_IsNotActive()
    {
        await _repo.AddAsync("First");
        var second = await _repo.AddAsync("Second");
        Assert.That(second.IsActive, Is.False);
    }

    [Test]
    public async Task Add_SetsName()
    {
        var config = await _repo.AddAsync("My Config");
        Assert.That(config.Name, Is.EqualTo("My Config"));
    }

    [Test]
    public async Task Add_WithCopyFrom_CopiesCalculationConfig()
    {
        var source = await _repo.AddAsync("Source");
        source.CalculationConfig = CalculationConfig.Default with
        {
            HouseSystem = HouseSystems.Koch
        };
        await _repo.UpdateAsync(source);

        var copy = await _repo.AddAsync("Copy", source);
        Assert.That(copy.CalculationConfig.HouseSystem, Is.EqualTo(HouseSystems.Koch));
    }

    // ── FetchAll ───────────────────────────────────────────────────────────

    [Test]
    public async Task FetchAll_EmptyDb_ReturnsEmptyList()
    {
        var all = await _repo.FetchAllAsync();
        Assert.That(all, Is.Empty);
    }

    [Test]
    public async Task FetchAll_ReturnsAllConfigs()
    {
        await _repo.AddAsync("Alpha");
        await _repo.AddAsync("Beta");
        var all = await _repo.FetchAllAsync();
        Assert.That(all.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task FetchAll_IsSortedByName()
    {
        await _repo.AddAsync("Zebra");
        await _repo.AddAsync("Alpha");
        var all = await _repo.FetchAllAsync();
        Assert.That(all[0].Name, Is.EqualTo("Alpha"));
        Assert.That(all[1].Name, Is.EqualTo("Zebra"));
    }

    // ── FetchActive ────────────────────────────────────────────────────────

    [Test]
    public async Task FetchActive_NoConfigs_ReturnsNull()
    {
        var active = await _repo.FetchActiveAsync();
        Assert.That(active, Is.Null);
    }

    [Test]
    public async Task FetchActive_ReturnsActiveConfig()
    {
        var added = await _repo.AddAsync("Only");
        var active = await _repo.FetchActiveAsync();
        Assert.That(active!.Id, Is.EqualTo(added.Id));
    }

    // ── Update ─────────────────────────────────────────────────────────────

    [Test]
    public async Task Update_PersistsChangedName()
    {
        var config = await _repo.AddAsync("Original");
        config.Name = "Renamed";
        await _repo.UpdateAsync(config);

        var fetched = (await _repo.FetchAllAsync()).First();
        Assert.That(fetched.Name, Is.EqualTo("Renamed"));
    }

    [Test]
    public async Task Update_PersistsCalculationConfig()
    {
        var config = await _repo.AddAsync("Test");
        config.CalculationConfig = CalculationConfig.Default with
        {
            HouseSystem = HouseSystems.Regiomontanus
        };
        await _repo.UpdateAsync(config);

        var fetched = (await _repo.FetchAllAsync()).First();
        Assert.That(fetched.CalculationConfig.HouseSystem, Is.EqualTo(HouseSystems.Regiomontanus));
    }

    [Test]
    public async Task Update_PersistsLanguage()
    {
        var config = await _repo.AddAsync("Test");
        config.Language = "nl";
        await _repo.UpdateAsync(config);

        var fetched = (await _repo.FetchAllAsync()).First();
        Assert.That(fetched.Language, Is.EqualTo("nl"));
    }

    // ── SetActive ──────────────────────────────────────────────────────────

    [Test]
    public async Task SetActive_DeactivatesOthers()
    {
        var first  = await _repo.AddAsync("First");
        var second = await _repo.AddAsync("Second");

        await _repo.SetActiveAsync(second.Id);

        var all = await _repo.FetchAllAsync();
        Assert.That(all.First(c => c.Id == first.Id).IsActive,  Is.False);
        Assert.That(all.First(c => c.Id == second.Id).IsActive, Is.True);
    }

    // ── Delete ─────────────────────────────────────────────────────────────

    [Test]
    public async Task Delete_OnlyConfig_ThrowsInvalidOperation()
    {
        var config = await _repo.AddAsync("Only");
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.DeleteAsync(config.Id));
    }

    [Test]
    public async Task Delete_ActiveConfig_ThrowsInvalidOperation()
    {
        var active  = await _repo.AddAsync("Active");   // IsActive = true (first)
        await _repo.AddAsync("Passive");                // second, IsActive = false
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.DeleteAsync(active.Id));
    }

    [Test]
    public async Task Delete_InactiveConfig_RemovesIt()
    {
        await _repo.AddAsync("Active");
        var passive = await _repo.AddAsync("Passive");

        await _repo.DeleteAsync(passive.Id);

        var all = await _repo.FetchAllAsync();
        Assert.That(all.Count,   Is.EqualTo(1));
        Assert.That(all[0].Name, Is.EqualTo("Active"));
    }
}

// ── Test helpers ──────────────────────────────────────────────────────────

/// <summary>
/// In-memory SQLite factory using a named shared-cache database so that multiple
/// short-lived connections all see the same data.
/// </summary>
internal sealed class InMemoryDbConnectionFactory : IDbConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    // Keep one connection open to prevent the in-memory DB from being destroyed.
    private readonly SqliteConnection _keepAlive;

    public InMemoryDbConnectionFactory()
    {
        var name = Guid.NewGuid().ToString("N");
        _connectionString = $"Data Source={name};Mode=Memory;Cache=Shared";
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    public SqliteConnection Create()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public void Dispose() => _keepAlive.Dispose();
}
