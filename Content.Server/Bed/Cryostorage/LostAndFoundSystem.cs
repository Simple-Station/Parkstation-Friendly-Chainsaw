using System.Linq;
using Content.Server.Hands.Systems;
using Content.Server.Inventory;
using Content.Server.Popups;
using Content.Shared.Access.Systems;
using Content.Shared.UserInterface;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Robust.Server.Containers;
using Robust.Server.GameObjects;

namespace Content.Server.Bed.Cryostorage;

/// <inheritdoc/>
public sealed class LostAndFoundSystem : SharedLostAndFoundSystem
{
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LostAndFoundComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpened);
        SubscribeLocalEvent<LostAndFoundComponent, CryostorageRemoveItemBuiMessage>(OnRemoveItemBuiMessage);
    }

    private void OnBeforeUIOpened(Entity<LostAndFoundComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateCryostorageUIState(ent);
    }

    private void OnRemoveItemBuiMessage(Entity<LostAndFoundComponent> ent, ref CryostorageRemoveItemBuiMessage args)
    {
        Log.Debug("on remove item bui message");

        var (_, comp) = ent;
        if (args.Session.AttachedEntity is not { } attachedEntity)
            return;

        var cryoContained = GetEntity(args.StoredEntity);

        if (!comp.StoredPlayers.Contains(cryoContained) || !IsInPausedMap(cryoContained))
            return;

        if (!HasComp<HandsComponent>(attachedEntity))
            return;

        if (!_accessReader.IsAllowed(attachedEntity, ent))
        {
            _popup.PopupEntity(Loc.GetString("cryostorage-popup-access-denied"), attachedEntity, attachedEntity);
            return;
        }

        EntityUid? entity = null;
        if (args.Type == CryostorageRemoveItemBuiMessage.RemovalType.Hand)
        {
            if (_hands.TryGetHand(cryoContained, args.Key, out var hand))
                entity = hand.HeldEntity;
        }
        else
        {
            if (_inventory.TryGetSlotContainer(cryoContained, args.Key, out var slot, out _))
                entity = slot.ContainedEntity;
        }

        if (entity == null)
            return;

        AdminLog.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(attachedEntity):player} removed item {ToPrettyString(entity)} from cryostorage-contained player " +
            $"{ToPrettyString(cryoContained):player}, stored in cryostorage {ToPrettyString(ent)}");
        _container.TryRemoveFromContainer(entity.Value);
        _transform.SetCoordinates(entity.Value, Transform(attachedEntity).Coordinates);
        _hands.PickupOrDrop(attachedEntity, entity.Value);
        UpdateCryostorageUIState(ent);
    }


    public void UpdateCryostorageUIState(Entity<LostAndFoundComponent> ent)
    {
        var state = new CryostorageBuiState(GetAllContainedData(ent).ToList());
        _ui.TrySetUiState(ent, CryostorageUIKey.Key, state);
    }

    private IEnumerable<CryostorageContainedPlayerData> GetAllContainedData(Entity<LostAndFoundComponent> ent)
    {
        foreach (var contained in ent.Comp.StoredPlayers)
        {
            yield return GetContainedData(contained);
        }
    }

    private CryostorageContainedPlayerData GetContainedData(EntityUid uid)
    {
        var data = new CryostorageContainedPlayerData();
        data.PlayerName = Name(uid);
        data.PlayerEnt = GetNetEntity(uid);

        var enumerator = _inventory.GetSlotEnumerator(uid);
        while (enumerator.NextItem(out var item, out var slotDef))
        {
            data.ItemSlots.Add(slotDef.Name, Name(item));
        }

        foreach (var hand in _hands.EnumerateHands(uid))
        {
            if (hand.HeldEntity == null)
                continue;

            data.HeldItems.Add(hand.Name, Name(hand.HeldEntity.Value));
        }

        return data;
    }

}
