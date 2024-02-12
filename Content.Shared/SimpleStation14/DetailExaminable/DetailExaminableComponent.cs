using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.DetailExaminable
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class DetailExaminableComponent : Component
    {
        [DataField("content", required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public string Content = "";
    }
}
