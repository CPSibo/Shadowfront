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
        var tree = GetTree();

        var damageLabel = new Label()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = $"{e.NewHealth - e.PreviousHealth:n0}",
            Position = new Vector2(20, -30), // TODO
        };
        damageLabel.AddThemeColorOverride("font_color", Colors.Red);
        damageLabel.AddThemeFontSizeOverride("font_size", 30);
        tree.Root.AddChild(damageLabel);

        var tween = damageLabel.CreateTween();
        tween.TweenProperty(damageLabel, "position", damageLabel.Position + new Vector2(0, -40), 1f)
            .SetTrans(Tween.TransitionType.Expo);

        tree.CreateTimer(1, processAlways: false)
            .Timeout += () => damageLabel.QueueFree();
    }
}
