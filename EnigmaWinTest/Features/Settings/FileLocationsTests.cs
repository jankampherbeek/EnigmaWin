// FileLocationsTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using EnigmaWin.Sources.Features.Settings;

namespace EnigmaWintest.Features.Settings;

/// <summary>Tests for FileLocations and AppLogging.</summary>
[TestFixture]
public class FileLocationsTests
{
    private string _root = string.Empty;
    private string SettingsPath => Path.Combine(_root, "settings.json");

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), "EnigmaWinTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [TearDown]
    public void TearDown()
    {
        Serilog.Log.CloseAndFlush();
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { /* ignore */ }
    }

    [Test]
    public void TestNothingSetGivesDefaults()
    {
        var locations = new FileLocations(SettingsPath);
        Assert.That(locations.DefaultResearchFolder, Is.Null);
        Assert.That(locations.ChosenLogFolder, Is.Null);
        Assert.That(locations.LogFolder, Is.EqualTo(FileLocations.DefaultLogFolder));
    }

    [Test]
    public void TestFoldersArePersisted()
    {
        var research = Directory.CreateDirectory(Path.Combine(_root, "research")).FullName;
        var logs     = Directory.CreateDirectory(Path.Combine(_root, "logs")).FullName;
        var locations = new FileLocations(SettingsPath);
        locations.SetDefaultResearchFolder(research);
        locations.SetLogFolder(logs);

        var reloaded = new FileLocations(SettingsPath);
        Assert.That(reloaded.DefaultResearchFolder, Is.EqualTo(research));
        Assert.That(reloaded.LogFolder, Is.EqualTo(logs));
        Assert.That(reloaded.LogFilePath, Is.EqualTo(Path.Combine(logs, FileLocations.LogFileName)));
    }

    [Test]
    public void TestClearRemovesSetting()
    {
        var research  = Directory.CreateDirectory(Path.Combine(_root, "research")).FullName;
        var locations = new FileLocations(SettingsPath);
        locations.SetDefaultResearchFolder(research);
        locations.SetDefaultResearchFolder(null);
        Assert.That(new FileLocations(SettingsPath).DefaultResearchFolder, Is.Null);
    }

    [Test]
    public void TestMissingFolderIsIgnored()
    {
        var locations = new FileLocations(SettingsPath);
        locations.SetLogFolder(Path.Combine(_root, "does-not-exist"));
        Assert.That(locations.ChosenLogFolder, Is.Null);
        Assert.That(locations.LogFolder, Is.EqualTo(FileLocations.DefaultLogFolder));
    }

    [Test]
    public void TestCorruptSettingsFileGivesDefaults()
    {
        File.WriteAllText(SettingsPath, "{ not json");
        Assert.That(new FileLocations(SettingsPath).DefaultResearchFolder, Is.Null);
    }

    [Test]
    public void TestLoggingWritesWarningsToChosenFolder()
    {
        var logs = Path.Combine(_root, "logs");
        Assert.That(AppLogging.Configure(logs), Is.EqualTo(logs));
        Serilog.Log.Information("not written");
        Serilog.Log.Warning("written {Value}", 42);
        Serilog.Log.CloseAndFlush();

        var file = AppLogging.LatestLogFile(logs);
        Assert.That(file, Is.Not.Null);
        var content = File.ReadAllText(file!);
        Assert.That(content, Does.Contain("written 42"));
        Assert.That(content, Does.Not.Contain("not written"));
    }
}
