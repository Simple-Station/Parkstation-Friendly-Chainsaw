using Content.Client.Chat.Managers;
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Client.Player;
using EmpathyChatComponent = Content.Shared.Parkstation.Species.Shadowkin.Components.EmpathyChatComponent;

namespace Content.Client.Parkstation.Chat
{
    public sealed class ShadowkinChatUpdateSystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EmpathyChatComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<EmpathyChatComponent, ComponentRemove>(OnRemove);
        }


        public EmpathyChatComponent? Player => CompOrNull<EmpathyChatComponent>(_playerManager.LocalPlayer?.ControlledEntity);
        public bool IsShadowkin => Player != null;

        private void OnInit(EntityUid uid, EmpathyChatComponent component, ComponentInit args)
        {
            _chatManager.UpdatePermissions();
        }

        private void OnRemove(EntityUid uid, EmpathyChatComponent component, ComponentRemove args)
        {
            _chatManager.UpdatePermissions();
        }
    }
}
