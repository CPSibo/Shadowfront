using Godot;
using Shadowfront.Backend;
using Shadowfront.Backend.Board;

public partial class PlayerCamera : Camera2D
{
    [Export]
    public float PanSpeed { get; set; } = 600f;

    [Export]
    public float RotateSpeed { get; set; } = 1f;

    [Export]
    public float ZoomSpeed { get; set; } = 0.5f;

    private Tween? _zoomTween = null;

    private Tween? _positionTween = null;

    public override void _Ready()
    {
        base._Ready();

        EventBus.Subscribe<GameBoard_BoardPieceActivatedEvent>(GameBoard_BoardPieceActivated);
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<GameBoard_BoardPieceActivatedEvent>(GameBoard_BoardPieceActivated);

        base._ExitTree();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        HandlePan(delta);

        HandleRotation(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        HandleZoom(@event);

        base._UnhandledInput(@event);
    }

    private void HandlePan(double delta)
    {
        var desiredPanVector = new Vector2(
            Input.GetActionStrength("GameBoard.PanCameraRight") - Input.GetActionStrength("GameBoard.PanCameraLeft"),
            Input.GetActionStrength("GameBoard.PanCameraDown") - Input.GetActionStrength("GameBoard.PanCameraUp")
        );

        if (desiredPanVector.IsZeroApprox())
            return;

        _positionTween?.Kill();

        desiredPanVector *= PanSpeed * (float)delta;

        Position += desiredPanVector;
    }

    private void HandleRotation(double delta)
    {
        var desiredRotationAmount = Input.GetActionStrength("GameBoard.RotateClockwise") 
            - Input.GetActionStrength("GameBoard.RotateCounterclockwise");

        if (Mathf.IsZeroApprox(desiredRotationAmount))
            return;

        desiredRotationAmount *= RotateSpeed * (float)delta;

        Rotate(desiredRotationAmount);
    }

    // TODO: Set predefined zoom levels to snap to, rather than
    //       letting the camera get stuck in off-pixel sizes.
    private void HandleZoom(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent)
            return;

        var newZoom = Zoom;

        if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
            newZoom += Vector2.One * ZoomSpeed;

        if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
            newZoom -= Vector2.One * ZoomSpeed;

        if (newZoom == Zoom)
            return;

        _zoomTween?.Kill();

        _zoomTween = CreateTween();
        _zoomTween.TweenProperty(this, "zoom", newZoom, 0.15f);
    }

    private void GameBoard_BoardPieceActivated(GameBoard_BoardPieceActivatedEvent e)
    {
        var boardPiecePosition = e.BoardPiece.Position;

        _positionTween?.Kill();

        _positionTween = CreateTween();
        _positionTween.TweenProperty(this, "position", boardPiecePosition, 0.5f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);
    }
}
