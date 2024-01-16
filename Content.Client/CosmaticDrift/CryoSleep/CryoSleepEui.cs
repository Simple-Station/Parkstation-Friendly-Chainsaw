using Content.Client.Eui;
using Content.Shared.CosmaticDrift.CryoSleep;
using Robust.Client.Graphics;

namespace Content.Client.CosmaticDrift.CryoSleep;

public sealed class CryoSleepEui : BaseEui
{
    private readonly AcceptCryoWindow _window;

    public CryoSleepEui()
    {
        _window = new AcceptCryoWindow();

        _window.DenyButton.OnPressed += _ =>
        {
            SendMessage(new AcceptCryoChoiceMessage(AcceptCryoUiButton.Deny));
            _window.Close();
        };

        _window.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new AcceptCryoChoiceMessage(AcceptCryoUiButton.Accept));
            _window.Close();
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

}
