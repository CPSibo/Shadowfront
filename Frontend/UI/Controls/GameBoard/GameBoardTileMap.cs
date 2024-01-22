using Godot;
using Shadowfront.Backend;
using Shadowfront.Backend.Board;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class GameBoardTileMap : TileMap
{
    public enum Styles
    {
        Default,
        Selected,
        Hovered,
        InRange,
    }

    public static ImmutableDictionary<Styles, Vector3I> StylesIndex { get; } = new Dictionary<Styles, Vector3I>()
    {
        { Styles.Default, new(2, 0, 0) },
        { Styles.Selected, new(2, 0, 2) },
        { Styles.Hovered, new(2, 0, 3) },
        { Styles.InRange, new(2, 0, 1) },
    }.ToImmutableDictionary();

    public const int GROUND_LAYER_INDEX = 0;

    public Vector2I TileSize { get; private set; }

    public Vector2I TileSizeHalf { get; private set; }

    public Vector2 TileSizeStacked { get; private set; }

    public GameBoard? GameBoard { get; private set; }

    private Dictionary<UnitToken, UnitTokenScene> _tokens = [];

    public override void _Ready()
    {
        TileSize = TileSet.TileSize;
        TileSizeHalf = TileSize / 2;
        TileSizeStacked = new Vector2(TileSize.X * 0.75f, TileSize.Y);

        CreateGameBoard();

        base._Ready();

        LateReadyAsync();
    }

    private async void LateReadyAsync()
    {
        await Task.Yield();

        if(GameBoard is not null)
            await GameBoard.LateReadyAsync();

        //SpawnToken(
        //    _cells[new(0, 0)],
        //    Game.Instance.Factions.First(f => f.Name == "Player"),
        //    Game.Instance.TokenTemplates.First(f => f is SoldierToken)
        //);

        //SpawnToken(
        //    _cells[new(0, -2)],
        //    Game.Instance.Factions.First(f => f.Name == "Enemy"),
        //    Game.Instance.TokenTemplates.First(f => f is SoldierToken)
        //);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var localMousePos = GetLocalMousePosition();
        var cellCoords = LocalToMap(localMousePos);

        HoverCell(cellCoords);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventMouseButton mouseevent)
        {
            if (mouseevent.ButtonIndex == MouseButton.Left && mouseevent.Pressed)
            {
                var screenSpaceClickLocation = GetLocalMousePosition();

                var mapSpaceClickLocation = LocalToMap(screenSpaceClickLocation);

                Debugger.Log(2, "Info", $"{mapSpaceClickLocation}\n");

                GameBoard?.CellPrimaryTouch(mapSpaceClickLocation);
            }

            if (mouseevent.ButtonIndex == MouseButton.Right && mouseevent.Pressed)
            {
                var screenSpaceClickLocation = GetLocalMousePosition();

                var mapSpaceClickLocation = LocalToMap(screenSpaceClickLocation);

                Debugger.Log(2, "Info", $"{mapSpaceClickLocation}\n");

                GameBoard?.CellSecondaryTouch(mapSpaceClickLocation);
            }
        }
    }

    public GameBoard CreateGameBoard()
    {
        var children = FindChildren("*");

        foreach(var child in children)
        {
            child.QueueFree();
        }

        var usedCells = GetUsedCells(GROUND_LAYER_INDEX);

        GameBoard = new([.. usedCells]);

        GameBoard.Disposing += GameBoard_Disposing;
        GameBoard.CellSelected += GameBoard_CellSelected;
        GameBoard.CellUnselected += GameBoard_CellUnselected;
        GameBoard.UnitTokenSelected += GameBoard_UnitTokenSelected;
        GameBoard.UnitTokenUnselected += GameBoard_UnitTokenUnselected;
        GameBoard.UnitTokenMoved += GameBoard_UnitTokenMoved;
        GameBoard.UnitTokenCreated += GameBoard_UnitTokenCreated;
        GameBoard.UnitTokenDisposing += GameBoard_UnitTokenDisposing;
        GameBoard.UnitTokenHealthChanged += GameBoard_UnitTokenHealthChanged;
        GameBoard.CellHovered += GameBoard_CellHovered;
        GameBoard.CellUnhovered += GameBoard_CellUnhovered;
        GameBoard.ShowUnitTokenMovementRange += GameBoard_ShowUnitTokenMovementRange;
        GameBoard.ClearUnitTokenMovementRange += GameBoard_ClearUnitTokenMovementRange;

        foreach (var cell in usedCells)
        {
            var localPos = GetCellLocalCoords(cell);

            var positionLabel = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"({cell.X}, {cell.Y})",
                Position = localPos - new Vector2(12, -20),
                ZIndex = 1,

            };
            positionLabel.AddThemeColorOverride("font_color", Colors.Black);
            positionLabel.AddThemeFontSizeOverride("font_size", 10);
            AddChild(positionLabel);
        }

        return GameBoard;
    }

    private void GameBoard_Disposing(DisposableRefCounted sender)
    {
        if (GameBoard is null)
            return;

        GameBoard.Disposing -= GameBoard_Disposing;
        GameBoard.CellSelected -= GameBoard_CellSelected;
        GameBoard.CellUnselected -= GameBoard_CellUnselected;
        GameBoard.UnitTokenSelected -= GameBoard_UnitTokenSelected;
        GameBoard.UnitTokenUnselected -= GameBoard_UnitTokenUnselected;
        GameBoard.UnitTokenMoved -= GameBoard_UnitTokenMoved;
        GameBoard.UnitTokenCreated -= GameBoard_UnitTokenCreated;
        GameBoard.UnitTokenDisposing -= GameBoard_UnitTokenDisposing;
        GameBoard.UnitTokenHealthChanged -= GameBoard_UnitTokenHealthChanged;
        GameBoard.CellHovered -= GameBoard_CellHovered;
        GameBoard.CellUnhovered -= GameBoard_CellUnhovered;
        GameBoard.ShowUnitTokenMovementRange -= GameBoard_ShowUnitTokenMovementRange;
        GameBoard.ClearUnitTokenMovementRange -= GameBoard_ClearUnitTokenMovementRange;
    }

    private void GameBoard_CellSelected(Vector2I cell)
    {
        SetCellStyle(cell, Styles.Selected);
    }

    private void GameBoard_CellUnselected(Vector2I cell)
    {
        RemoveCellStyles(cell, Styles.Selected);
    }

    private void GameBoard_CellHovered(Vector2I cell)
    {
        SetCellStyle(cell, Styles.Hovered);
    }

    private void GameBoard_CellUnhovered(Vector2I cell)
    {
        RemoveCellStyles(cell, Styles.Hovered);
    }

    private void GameBoard_ShowUnitTokenMovementRange(Godot.Collections.Array<Vector2I> cells)
    {
        foreach (var cell in cells)
        {
            SetCellStyle(cell, Styles.InRange);
        }
    }

    private void GameBoard_ClearUnitTokenMovementRange(Godot.Collections.Array<Vector2I> cells)
    {
        foreach(var cell in cells)
        {
            RemoveCellStyles(cell, Styles.InRange);
        }
    }

    private void GameBoard_UnitTokenSelected(UnitToken token)
    {
        Debugger.Log(2, "Info", "Unit token selected\n");
    }

    private void GameBoard_UnitTokenUnselected(UnitToken token)
    {
        Debugger.Log(2, "Info", "Unit token unselected\n");
    }

    private void GameBoard_UnitTokenMoved(UnitToken token, Vector2I from, Vector2I to)
    {
        var tokenScene = _tokens[token];
        tokenScene.Position = GetCellLocalCoords(to);
    }

    private void GameBoard_UnitTokenCreated(UnitToken token, Vector2I cell)
    {
        var unitTokenScenePath = "res://Frontend/UI/Controls/GameBoard/UnitTokenScene.tscn";

        var tokenPacked = GD.Load<PackedScene>(token.UnitResource.UnitTokenScene);
        var tokenScene = tokenPacked.Instantiate() as UnitTokenScene;

        if (tokenScene is null)
            throw new Exception($"Resource path could not be loaded: {unitTokenScenePath}");

        _tokens.Add(token, tokenScene);

        tokenScene.Position = GetCellLocalCoords(cell);
        tokenScene.ZIndex = 1;

        //var sprite = tokenScene.GetNode<Sprite2D>("Sprite2D");

        //if (sprite is null)
        //    throw new Exception("Could not find sprite");

        //var texture = GD.Load<CompressedTexture2D>(spritePath);
        //sprite.Texture = texture;

        //sprite.Modulate = token.Team is PlayerFactionResource
        //    ? Colors.LightBlue
        //    : Colors.Pink;

        var sprite = tokenScene.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        if (sprite is null)
            throw new Exception("Could not find sprite");

        sprite.Play();

        AddChild(tokenScene);
    }

    private void GameBoard_UnitTokenDisposing(UnitToken token)
    {
        var tokenScene = _tokens[token];

        tokenScene.QueueFree();

        _tokens.Remove(token);
    }

    private void GameBoard_UnitTokenHealthChanged(UnitToken token, float previousHealth, float newHealth)
    {
        var tokenScene = _tokens[token];

        var damageLabel = new Label()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = $"{newHealth - previousHealth:n0}",
            Position = tokenScene.Position + new Vector2(20, -30),
        };
        damageLabel.AddThemeColorOverride("font_color", Colors.Red);
        damageLabel.AddThemeFontSizeOverride("font_size", 30);
        GetTree().Root.AddChild(damageLabel);

        var tween = damageLabel.CreateTween();
        tween.TweenProperty(damageLabel, "position", damageLabel.Position + new Vector2(0, -40), 1f).SetTrans(Tween.TransitionType.Expo);

        var timer = GetTree().CreateTimer(1);
        timer.Timeout += () => damageLabel.QueueFree();
    }

    public Vector2 GetCellLocalCoords(Vector2I cell)
    {
        return MapToLocal(cell);
    }

    public Vector2 GetCellGlobalCoords(Vector2I cell)
    {
        var localCoords = MapToLocal(cell);

        return ToGlobal(localCoords);
    }

    public void SetCellStyle(Vector2I cell, Styles style)
    {
        var styleIndex = StylesIndex[style];

        SetCell(
            GROUND_LAYER_INDEX,
            cell,
            sourceId: 1,
            atlasCoords: new(styleIndex.X, styleIndex.Y),
            alternativeTile: styleIndex.Z
        );
    }

    public bool CellIsStyle(Vector2I cell, Styles style)
    {
        var styleIndex = StylesIndex[style];

        if (GetCellAtlasCoords(GROUND_LAYER_INDEX, cell) != new Vector2I(styleIndex.X, styleIndex.Y))
            return false;

        if (GetCellAlternativeTile(GROUND_LAYER_INDEX, cell) != styleIndex.Z)
            return false;

        return true;
    }

    public void RemoveCellStyles(Vector2I cell, IEnumerable<Styles> styles)
    {
        foreach (var style in styles)
        {
            if (!CellIsStyle(cell, style))
                continue;

            SetCellStyle(cell, Styles.Default);

            return;
        }
    }

    public void RemoveCellStyles(Vector2I cell, params Styles[] styles)
    {
        RemoveCellStyles(cell, styles.AsEnumerable());
    }

    public void HoverCell(Vector2I cell)
    {
        if (GameBoard is null)
            throw new Exception("Game board is null");

        GameBoard.OnCellMouseOver(cell);
    }
}
