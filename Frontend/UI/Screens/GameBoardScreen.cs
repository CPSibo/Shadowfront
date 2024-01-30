using Godot;
using Shadowfront.Backend;
using Shadowfront.Backend.Board.BoardPieces.Behaviors;

public partial class GameBoardScreen : Node2D
{
    public override void _Ready()
    {
        base._Ready();

        EventBus.Subscribe<BoardPiece_HealthChangedEvent>(BoardPiece_HealthChanged);
    }

    ~GameBoardScreen()
    {
        EventBus.Unsubscribe<BoardPiece_HealthChangedEvent>(BoardPiece_HealthChanged);
    }

    private void BoardPiece_HealthChanged(BoardPiece_HealthChangedEvent e)
    {
        var sourcePosition = e.BoardPiece.Position;

        AddDamageLabel(sourcePosition, e.NewHealth - e.PreviousHealth);
        AddBoardPieceDialog(sourcePosition, e.NewHealth > 0 ? "Taking fire!" : "I'm hit!");
    }

    private Label AddDamageLabel(Vector2 position, float damageAmount)
    {
        var tree = GetTree();

        var label = new Label()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = $"{damageAmount:n0}",
            Position = position + new Vector2(-30, -30),
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
