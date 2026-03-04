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
