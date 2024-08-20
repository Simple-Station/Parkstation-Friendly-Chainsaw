using Content.Client.Parkstation.Overlays.Shaders;
using Content.Shared.Humanoid;
using Content.Shared.Parkstation.Species.Shadowkin.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Parkstation.Species.Shadowkin.Systems;

public sealed class ShadowkinDarkSwappedSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;

    private IgnoreHumanoidWithComponentOverlay _ignoreOverlay = default!;
    private EtherealOverlay _etherealOverlay = default!;


    public override void Initialize()
    {
        base.Initialize();

        _ignoreOverlay = new IgnoreHumanoidWithComponentOverlay();
        _ignoreOverlay.IgnoredComponents.Add(new HumanoidAppearanceComponent());
        _ignoreOverlay.AllowAnywayComponents.Add(new ShadowkinDarkSwappedComponent());
        _etherealOverlay = new EtherealOverlay();

        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }


    private void OnStartup(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentStartup args)
    {
        if (_player.LocalEntity != uid)
            return;

        AddOverlay();
    }

    private void OnShutdown(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity != uid)
            return;

        RemoveOverlay();
    }

    private void OnPlayerAttached(EntityUid uid, ShadowkinDarkSwappedComponent component, LocalPlayerAttachedEvent args)
    {
        AddOverlay();
    }

    private void OnPlayerDetached(EntityUid uid, ShadowkinDarkSwappedComponent component, LocalPlayerDetachedEvent args)
    {
        RemoveOverlay();
    }


    private void AddOverlay()
    {
        _overlay.AddOverlay(_ignoreOverlay);
        _overlay.AddOverlay(_etherealOverlay);
    }

    private void RemoveOverlay()
    {
        _ignoreOverlay.Reset();
        _overlay.RemoveOverlay(_ignoreOverlay);
        _overlay.RemoveOverlay(_etherealOverlay);
    }
}
