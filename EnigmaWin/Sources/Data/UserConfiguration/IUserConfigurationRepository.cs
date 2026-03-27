// IUserConfigurationRepository.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Data.UserConfiguration;

public interface IUserConfigurationRepository
{
    /// <summary>Returns all configurations sorted by name.</summary>
    Task<IReadOnlyList<Features.Config.UserConfiguration>> FetchAllAsync();

    /// <summary>Returns the currently active configuration, or null if none exists.</summary>
    Task<Features.Config.UserConfiguration?> FetchActiveAsync();

    /// <summary>
    /// Adds a new configuration. If it is the first configuration it is automatically set as active.
    /// Pass <paramref name="copyFrom"/> to copy sub-configs from an existing configuration.
    /// </summary>
    Task<Features.Config.UserConfiguration> AddAsync(string name,
        Features.Config.UserConfiguration? copyFrom = null);

    /// <summary>Persists changes to an existing configuration.</summary>
    Task UpdateAsync(Features.Config.UserConfiguration config);

    /// <summary>
    /// Sets the given configuration as active and deactivates all others.
    /// </summary>
    Task SetActiveAsync(Guid id);

    /// <summary>
    /// Deletes a configuration.
    /// Throws <see cref="InvalidOperationException"/> when the configuration is active or is the only one.
    /// </summary>
    Task DeleteAsync(Guid id);
}
