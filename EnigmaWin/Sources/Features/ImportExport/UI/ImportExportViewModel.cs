// ImportExportViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Data.Event;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Features.ImportExport.Aaf;
using EnigmaWin.Sources.Features.ImportExport.Enigma;
using EnigmaWin.Sources.Features.ImportExport.Qck;
using EnigmaWin.Sources.Features.ImportExport.Shared;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.ImportExport.UI;

/// <summary>The supported import/export formats.</summary>
public enum ImportExportFormat
{
    Enigma,
    QuickChart,
    Aaf97
}

/// <summary>
/// ViewModel for the import/export screen of one format. The view shows the file dialogs and passes the
/// chosen path; this ViewModel reads or writes the file and reports the outcome. Only fatal errors (records
/// that could not be imported or exported at all) are shown, warnings are not.
/// </summary>
public sealed partial class ImportExportViewModel : ObservableObject
{
    private readonly IRosetta _rosetta;
    private readonly IHoroscopeRepository _horoscopeRepository;
    private readonly IEventRepository _eventRepository;

    public ImportExportFormat Format { get; }

    public string Title       { get; }
    public string Body        { get; }
    public string LabelExport { get; }
    public string LabelImport { get; }

    /// <summary>File dialog filter, e.g. "QuickChart file (*.qck)|*.qck|All files (*.*)|*.*".</summary>
    public string FileFilter { get; }
    public string DefaultExtension { get; }
    public string DefaultFileName => "enigma-export" + DefaultExtension;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? _statusMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    private bool _isBusy;

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);
    public bool HasError  => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsIdle    => !IsBusy;

    public ImportExportViewModel(
        ImportExportFormat format,
        IRosetta rosetta,
        IHoroscopeRepository horoscopeRepository,
        IEventRepository eventRepository)
    {
        Format = format;
        _rosetta = rosetta;
        _horoscopeRepository = horoscopeRepository;
        _eventRepository = eventRepository;

        var (titleKey, bodyKey, filterKey, extension) = format switch
        {
            ImportExportFormat.QuickChart => (ImportExportKeys.QckTitle, ImportExportKeys.QckBody, ImportExportKeys.QckFilter, ".qck"),
            ImportExportFormat.Aaf97      => (ImportExportKeys.AafTitle, ImportExportKeys.AafBody, ImportExportKeys.AafFilter, ".aaf"),
            _                             => (ImportExportKeys.EnigmaTitle, ImportExportKeys.EnigmaBody, ImportExportKeys.EnigmaFilter, ".json")
        };
        Title = T(titleKey);
        Body = T(bodyKey);
        LabelExport = T(ImportExportKeys.ExportButton);
        LabelImport = T(ImportExportKeys.ImportButton);
        DefaultExtension = extension;
        FileFilter = $"{T(filterKey)} (*{extension})|*{extension}|{T(ImportExportKeys.FilterAllFiles)} (*.*)|*.*";
    }

    /// <summary>Exports all charts (and, for the Enigma format, events) to <paramref name="path"/>.</summary>
    public async Task ExportToFileAsync(string path)
    {
        await RunAsync(async () =>
        {
            FormatExportResult result;
            try
            {
                result = Format switch
                {
                    ImportExportFormat.QuickChart => await new QckImportExportOrchestrator(_horoscopeRepository).ExportAsync(),
                    ImportExportFormat.Aaf97      => await new AafImportExportOrchestrator(_horoscopeRepository).ExportAsync(),
                    _ => await new EnigmaImportExportOrchestrator(_horoscopeRepository, _eventRepository).ExportAsync()
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = Localized(ImportExportKeys.ErrorDatabase, ex.Message);
                return;
            }

            try
            {
                await File.WriteAllBytesAsync(path, result.Data);
            }
            catch (Exception ex)
            {
                ErrorMessage = Localized(ImportExportKeys.ErrorWriteFile, ex.Message);
                return;
            }

            StatusMessage = Format is ImportExportFormat.Enigma
                ? Localized(ImportExportKeys.EnigmaExportSuccess, result.ChartsExported, result.EventsExported)
                : Localized(Format == ImportExportFormat.QuickChart ? ImportExportKeys.QckExportSuccess : ImportExportKeys.AafExportSuccess,
                    result.ChartsExported);
            ErrorMessage = DescribeFatalMessages(result.FatalMessages);
        });
    }

    /// <summary>Imports charts (and, for the Enigma format, events) from <paramref name="path"/>.</summary>
    public async Task ImportFromFileAsync(string path)
    {
        await RunAsync(async () =>
        {
            byte[] data;
            try
            {
                data = await File.ReadAllBytesAsync(path);
            }
            catch (Exception ex)
            {
                ErrorMessage = Localized(ImportExportKeys.ErrorReadFile, ex.Message);
                return;
            }

            FormatImportResult result;
            try
            {
                result = Format switch
                {
                    ImportExportFormat.QuickChart => await new QckImportExportOrchestrator(_horoscopeRepository).ImportAsync(data),
                    ImportExportFormat.Aaf97      => await new AafImportExportOrchestrator(_horoscopeRepository).ImportAsync(data),
                    _ => await new EnigmaImportExportOrchestrator(_horoscopeRepository, _eventRepository).ImportAsync(data)
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = Localized(ImportExportKeys.ErrorDatabase, ex.Message);
                return;
            }

            StatusMessage = Format switch
            {
                ImportExportFormat.QuickChart => Localized(ImportExportKeys.QckImportSuccess, result.ChartsImported),
                ImportExportFormat.Aaf97      => Localized(ImportExportKeys.AafImportSuccess, result.ChartsImported, result.ChartsSkipped),
                _ => Localized(ImportExportKeys.EnigmaImportSuccess,
                    result.ChartsImported, result.EventsImported, result.ChartsSkipped, result.EventsSkipped)
            };
            ErrorMessage = DescribeFatalMessages(result.FatalMessages);
        });
    }

    private async Task RunAsync(Func<Task> action)
    {
        IsBusy = true;
        StatusMessage = null;
        ErrorMessage = null;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string? DescribeFatalMessages(System.Collections.Generic.IEnumerable<ExchangeMessage> messages)
    {
        var lines = messages.Select(Describe).ToList();
        return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
    }

    /// <summary>Resolves the localized text of a message, prefixed with its record number when it has one.</summary>
    public string Describe(ExchangeMessage message)
    {
        var text = Localized(message.Key, message.Args.ToArray());
        return message.RecordNumber is { } recordNumber
            ? Localized(ImportExportKeys.RecordMessage, recordNumber, text)
            : text;
    }

    private string T(string key) => _rosetta.GetText(RbFile.ImportExport, key);

    private string Localized(string key, params object[] args)
    {
        var template = T(key);
        try
        {
            return string.Format(CultureInfo.CurrentCulture, template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }
}
