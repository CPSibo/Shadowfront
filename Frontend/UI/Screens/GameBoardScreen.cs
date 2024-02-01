using Godot;
using Shadowfront.Backend;
using static Shadowfront.Backend.Board.BoardPieces.ObjectAttributes;

public partial class GameBoardScreen : Node2D
{
    public override void _Ready()
    {
        base._Ready();

        EventBus.Subscribe<ObjectAttributes_AttributeValueCurrentChangedEvent>(ObjectAttributes_AttributeValueCurrentChanged);
    }

    ~GameBoardScreen()
    {
        EventBus.Unsubscribe<ObjectAttributes_AttributeValueCurrentChangedEvent>(ObjectAttributes_AttributeValueCurrentChanged);
    }

    private void ObjectAttributes_AttributeValueCurrentChanged(ObjectAttributes_AttributeValueCurrentChangedEvent e)
    {
        if (e.Owner is not Node2D owner2D)
            return;

        var sourcePosition = owner2D.Position;

        AddDamageLabel(sourcePosition, e.NewValue - e.PreviousValue);
        AddBoardPieceDialog(sourcePosition, e.NewValue > 0f ? "Taking fire!" : "I'm hit!");
    }

    private Label AddDamageLabel(Vector2 position, float damageAmount)
    {
        var tree = GetTree();

        var label = new Label()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = $"{damageAmount:n0}",
            Position = position + new Vector2(-60, -30),
        };
        label.AddThemeColorOverride("font_color", Colors.Red);
        label.AddThemeFontSizeOverride("font_size", 30);
        tree.Root.AddChild(label);

        var tween = label.CreateTween();
        tween.TweenProperty(label, "position", label.Position + new Vector2(0, -40), 0.75f)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);

        tree.CreateTimer(0.75d, processAlways: false)
            .Timeout += () => label.QueueFree();

        return label;
    }

    private Label AddBoardPieceDialog(Vector2 position, string text)
    {
        var tree = GetTree();

        var label = new Label()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            Text = text,
            Position = position + new Vector2(30, -30),
            ZIndex = 5,
            ThemeTypeVariation = "BoardPieceDialogLabel"
        };
        tree.Root.AddChild(label);

        tree.CreateTimer(1d, processAlways: false)
            .Timeout += () => label.QueueFree();

        return label;
    }
}
