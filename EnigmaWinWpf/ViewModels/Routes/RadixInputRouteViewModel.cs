// RadixInputRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.ViewModels.Routes;

public sealed class RadixInputRouteViewModel
{
    public RadixInputRouteViewModel(Guid sessionId)
    {
        SessionId = sessionId;
    }

    public Guid SessionId { get; }
}
