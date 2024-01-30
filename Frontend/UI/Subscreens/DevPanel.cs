using Godot;
using System;

public partial class DevPanel : Control
{
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        base._UnhandledKeyInput(@event);

        if (@event is not InputEventKey key)
            return;

        if (key.Keycode == Key.F12 && key.Pressed)
            Visible = !Visible;
    }
}
