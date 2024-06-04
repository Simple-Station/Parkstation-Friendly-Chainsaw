using Content.Shared.Abilities;
using Content.Shared.DeltaV.CCVars;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;

namespace Content.Client.DeltaV.Overlays;

public sealed partial class UltraVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private UltraVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UltraVisionComponent, ComponentInit>(OnUltraVisionInit);
        SubscribeLocalEvent<UltraVisionComponent, ComponentShutdown>(OnUltraVisionShutdown);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnUltraVisionInit(EntityUid uid, UltraVisionComponent component, ComponentInit args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnUltraVisionShutdown(EntityUid uid, UltraVisionComponent component, ComponentShutdown args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
