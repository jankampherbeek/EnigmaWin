// ConfigListViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class ConfigListViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService _nav;
    private readonly IConfigContext _configContext;
    private readonly IRosetta _rosetta;

    public ObservableCollection<UserConfiguration> Configurations { get; } = [];

    [ObservableProperty]
    private UserConfiguration? _selectedConfig;

    [ObservableProperty]
    private bool _isAddPanelVisible;

    [ObservableProperty]
    private string _newConfigName = string.Empty;

    [ObservableProperty]
    private UserConfiguration? _basedOnConfig;

    // ── Localized labels ────────────────────────────────────────────────────

    public string LabelTitle        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.list.title");
    public string LabelAdd          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.list.new");
    public string LabelActiveBadge  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.list.activebadge");
    public string LabelDeleteTitle  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.list.alert.deletetitle");
    public string LabelDeleteButton => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.list.alert.deletebutton");
    public string LabelDeleteMessage => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.list.alert.deletemessage");
    public string LabelCancel       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelNewTitle     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.new.sheettitle");
    public string LabelNewName      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.new.namelabel");
    public string LabelNewNamePlaceholder => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.new.nameplaceholder");
    public string LabelNewBasedOn   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.new.basedon");
    public string LabelNewBasedOnFooter => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.new.basedonfooter");
    public string LabelNewDefaults  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.new.defaults");
    public string LabelNewCreate    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.new.create");
    public string LabelEmptyTitle   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.empty.title");
    public string LabelEmptyDesc    => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.empty.description");

    public ConfigListViewModel(
        IUserConfigurationRepository repo,
        INavigationService nav,
        IConfigContext configContext,
        IRosetta rosetta)
    {
        _repo = repo;
        _nav = nav;
        _configContext = configContext;
        _rosetta = rosetta;
        if (_configContext is INotifyPropertyChanged notifyConfig)
            notifyConfig.PropertyChanged += OnConfigContextPropertyChanged;

        _ = LoadAsync();
    }

    private void OnConfigContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConfigContext.ActiveConfig))
            _ = LoadAsync();
    }

    // ── Data loading ────────────────────────────────────────────────────────

    private async Task LoadAsync()
    {
        var all = await _repo.FetchAllAsync();
        Configurations.Clear();
        foreach (var c in all)
            Configurations.Add(c);

        // Auto-select the active config, or the first one available
        var autoSelect = Configurations.FirstOrDefault(c => c.IsActive)
                      ?? Configurations.FirstOrDefault();
        if (autoSelect is not null)
            SelectConfig(autoSelect);
    }

    // ── Selection ───────────────────────────────────────────────────────────

    internal void SelectConfig(UserConfiguration config)
    {
        SelectedConfig = config;
        _configContext.EditingConfig = config;
        _nav.NavigateDetail(AppRoutes.ConfigEdit, new ConfigEditNavigationParameter(config.Id));
    }

    // ── Add config ──────────────────────────────────────────────────────────

    internal async Task AddConfigAsync()
    {
        var name = NewConfigName.Trim();
        if (string.IsNullOrEmpty(name)) return;

        var newConfig = await _repo.AddAsync(name, BasedOnConfig);

        // Insert in alphabetical order
        var insertIndex = Configurations
            .TakeWhile(c => string.Compare(c.Name, newConfig.Name, StringComparison.OrdinalIgnoreCase) < 0)
            .Count();
        Configurations.Insert(insertIndex, newConfig);

        IsAddPanelVisible = false;
        NewConfigName = string.Empty;
        BasedOnConfig = null;

        SelectConfig(newConfig);
    }

    internal void CancelAdd()
    {
        IsAddPanelVisible = false;
        NewConfigName = string.Empty;
        BasedOnConfig = null;
    }

    // ── Delete config ───────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to delete <paramref name="config"/>. Silently ignores if deletion is
    /// not allowed (active config or last remaining config).
    /// </summary>
    internal async Task DeleteConfigAsync(UserConfiguration config)
    {
        try
        {
            await _repo.DeleteAsync(config.Id);
            Configurations.Remove(config);

            if (SelectedConfig?.Id == config.Id)
            {
                var next = Configurations.FirstOrDefault(c => c.IsActive)
                        ?? Configurations.FirstOrDefault();
                if (next is not null)
                    SelectConfig(next);
                else
                {
                    SelectedConfig = null;
                    _configContext.EditingConfig = null;
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Deletion was prevented (active or last config) — ignore silently
        }
    }

    // ── Set active ──────────────────────────────────────────────────────────

    internal async Task SetActiveAsync(UserConfiguration config)
    {
        await _repo.SetActiveAsync(config.Id);
        _configContext.ActiveConfig = config;
        await LoadAsync();
    }

    // ── Formatted delete message ─────────────────────────────────────────────

    public string FormatDeleteMessage(string configName) =>
        string.Format(LabelDeleteMessage, configName);
}
