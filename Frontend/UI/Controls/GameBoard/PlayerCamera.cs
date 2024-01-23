using Godot;

public partial class PlayerCamera : Camera2D
{
    [Export]
    public float PanSpeed { get; set; } = 600f;

    [Export]
    public float RotateSpeed { get; set; } = 1f;

    [Export]
    public float ZoomSpeed { get; set; } = 0.5f;

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

    private void HandleZoom(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent)
            return;

        if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
            Zoom += Vector2.One * ZoomSpeed;

        if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
            Zoom -= Vector2.One * ZoomSpeed;
    }
}
