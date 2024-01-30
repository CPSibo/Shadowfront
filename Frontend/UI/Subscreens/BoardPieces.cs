using Godot;
using Godot.Collections;
using Shadowfront.Backend;
using Shadowfront.Backend.Board;
using System;

public partial class BoardPieces : GridContainer
{
    private Button _createBoardPieceButton = null!;

    private OptionButton _boardPieceDropdown = null!;

    private OptionButton _teamDropdown = null!;

    private SpinBox _x = null!;

    private SpinBox _y = null!;

    private Dictionary<int, string> _teams = new() {
        { 0, "player" },
        { 1, "enemy" },
    };

    private Dictionary<int, string> _boardPieces = new() {
        { 0, "claire" },
    };

    public override void _Ready()
    {
        base._Ready();

        _boardPieceDropdown = FindChild("BoardPiece") as OptionButton
            ?? throw new Exception("Cannot find BoardPiece dropdown");

        _boardPieceDropdown.Clear();

        foreach (var boardPiece in _boardPieces)
        {
            _boardPieceDropdown.AddItem(boardPiece.Value, boardPiece.Key);
        }



        _teamDropdown = FindChild("Team") as OptionButton
            ?? throw new Exception("Cannot find Team button");

        _teamDropdown.Clear();

        foreach (var team in _teams)
        {
            _teamDropdown.AddItem(team.Value, team.Key);
        }



        _x = FindChild("X") as SpinBox
            ?? throw new Exception("Cannot find X input");



        _y = FindChild("Y") as SpinBox
            ?? throw new Exception("Cannot find Y input");



        _createBoardPieceButton = FindChild("CreateBoardPiece") as Button
            ?? throw new Exception("Cannot find CreateBoardPiece button");

        _createBoardPieceButton.Pressed += _createBoardPieceButton_Pressed;
    }

    private void _createBoardPieceButton_Pressed()
    {
        var x = (int)_x.Value;
        var y = (int)_y.Value;

        if (!_teams.TryGetValue(_teamDropdown.Selected, out var team))
            return;

        if (!_boardPieces.TryGetValue(_boardPieceDropdown.Selected, out var boardPiece))
            return;

        var boardPieceScenePath = boardPiece switch
        {
            "claire" => "res://Frontend/UI/Controls/GameBoard/UnitTokens/ClaireUnitTokenScene.tscn",
            _ => throw new NotImplementedException()
        };

        EventBus.Emit(new GameBoard_BoardPieceCreationRequestedEvent(boardPieceScenePath, team, new(x, y)));
    }
}
