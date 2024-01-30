using Godot;
using System;

public partial class BoardPieceActionBarButton : Control
{
    [Export]
    public string Label { get; set; } = "?";

    public override void _Ready()
    {
        base._Ready();

        var label = FindChild("Label") as Label
            ?? throw new Exception($"Cannot find label");

        label.Text = Label;
    }
}
