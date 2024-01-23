using Godot;
using Shadowfront.Backend.Utilities;
using System.Collections.Generic;
using System.Linq;

public partial class BoardPieceMovement : Node
{
    [Signal]
    public delegate void PositionChangedEventHandler(Vector2I previousPosition, Vector2I newPosition);

    private Vector2I _position;

    /// <summary>
    /// The item's current position.
    /// </summary>
    public Vector2I Position
    {
        get => _position;
    }

    /// <summary>
    /// The maximum number of tiles the piece can move.
    /// </summary>
    [Export]
    public int MaximumMoveRange { get; set; }

    // TODO: Not implemented.
    /// <summary>
    /// The minimum number of tiles the piece can move.
    /// </summary>
    [Export]
    public int MinimumMoveRange { get; set; }

    /// <summary>
    /// Whether the current piece can attempt to move.
    /// </summary>
    public bool CanMove => MaximumMoveRange > 0;

    private HashSet<Vector2I> _availableCells = [];

    private HashSet<Vector2I> _cellsInrange = [];

    public HashSet<Vector2I> CellsInRange => _cellsInrange;

    public void SetAvailableCells(IEnumerable<Vector2I> availableCells)
    {
        _availableCells = [..availableCells];

        _cellsInrange = ComputeMovementTargets();
    }

    /// <summary>
    /// Compute the valid movement targets for the piece.
    /// </summary>
    private HashSet<Vector2I> ComputeMovementTargets()
    {
        if (!CanMove)
            return [];

        return HexTileMapUtils
            .GetActualCellsWithinRange(_availableCells, Position, MaximumMoveRange)
            .ToHashSet();
    }

    /// <summary>
    /// Forces the position to the new value.
    /// </summary>
    public void ForcePosition(Vector2I desiredPosition)
    {
        var previousPosition = _position;

        _position = desiredPosition;

        _cellsInrange = ComputeMovementTargets();

        EmitSignal(SignalName.PositionChanged, previousPosition, _position);
    }

    /// <summary>
    /// Attempts to update the position to the new value.
    /// </summary>
    /// <remarks>
    /// <para>Will check against the valid movement targets within <see cref="CellsInRange"/>.</para>
    /// </remarks>
    /// <param name="desiredPosition">Target movement cell.</param>
    /// <returns>Whether the movement was allowed.</returns>
    public bool MoveTo(Vector2I desiredPosition)
    {
        if(!CanMove)
            return false;

        if(desiredPosition == _position)
            return false;

        if (!_cellsInrange.Contains(desiredPosition))
            return false;

        var previousPosition = _position;

        _position = desiredPosition;

        _cellsInrange = ComputeMovementTargets();

        EmitSignal(SignalName.PositionChanged, previousPosition, _position);

        return true;
    }
}
