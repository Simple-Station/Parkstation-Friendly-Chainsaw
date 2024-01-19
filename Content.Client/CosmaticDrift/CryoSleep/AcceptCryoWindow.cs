using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.CryoSleep;
public sealed class AcceptCryoWindow : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button AcceptButton;
    public readonly BoxContainer ButtonBox;
    public readonly BoxContainer InnerBox;

    public AcceptCryoWindow()
    {

        Title = Loc.GetString("accept-cryo-window-title");

        AcceptButton = new Button() {Text = Loc.GetString("accept-cryo-window-accept-button")};
        DenyButton = new Button() { Text = Loc.GetString("accept-cryo-window-deny-button")};

        // This one holds the buttons
        ButtonBox = new BoxContainer()
        {
            Orientation = LayoutOrientation.Horizontal,
            Align = AlignMode.Center,
            Children = {AcceptButton, (new Control() { MinSize = new Vector2i(20, 0) }), DenyButton}
        };

        // This one holds the button container
        InnerBox = new BoxContainer()
        {
            Orientation = LayoutOrientation.Vertical,
            Children = {(new Label() {Text = Loc.GetString("accept-cryo-window-prompt-text-part")}), ButtonBox}
        };

        // Put it all together
        Contents.AddChild(new BoxContainer { Orientation = LayoutOrientation.Vertical, Children = { InnerBox } });
    }
}
