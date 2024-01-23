using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Shadowfront.Backend.Board
{
    public partial class GameBoard : TileMap, IDisposableNode
    {
        [Signal]
        public delegate void DisposingEventHandler(GameBoard sender);

        [Signal]
        public delegate void CellSelectedEventHandler(Vector2I cell);

        [Signal]
        public delegate void CellUnselectedEventHandler(Vector2I cell);

        [Signal]
        public delegate void CellHoveredEventHandler(Vector2I cell);

        [Signal]
        public delegate void CellUnhoveredEventHandler(Vector2I cell);

        [Signal]
        public delegate void UnitTokenSelectedEventHandler(UnitToken token);

        [Signal]
        public delegate void UnitTokenUnselectedEventHandler(UnitToken token);

        [Signal]
        public delegate void UnitTokenMovedEventHandler(UnitToken token, Vector2I from, Vector2I to);

        [Signal]
        public delegate void UnitTokenCreatedEventHandler(UnitToken token, Vector2I cell);

        [Signal]
        public delegate void UnitTokenHealthChangedEventHandler(UnitToken token, float previousHealth, float newHealth);

        [Signal]
        public delegate void UnitTokenHealthReachedZeroEventHandler(UnitToken token);

        [Signal]
        public delegate void UnitTokenDisposingEventHandler(UnitToken token);

        [Signal]
        public delegate void ShowUnitTokenMovementRangeEventHandler(Godot.Collections.Array<Vector2I> cells);

        [Signal]
        public delegate void ClearUnitTokenMovementRangeEventHandler(Godot.Collections.Array<Vector2I> cells);
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

        public int GROUND_LAYER_INDEX = 0;

        public int DEFAULT_TILESET_ID = 0;

        public Dictionary<Vector2I, GameBoardCell> Cells { get; private set; } = [];

        private readonly List<UnitToken> _tokens = [];

        private readonly Dictionary<UnitToken, GameBoardCell> _tokenCells = [];

        public GameBoardCell? SelectedCell { get; private set; }

        public GameBoardCell? HoveredCell { get; private set; }

        public HashSet<GameBoardCell>? CellsWithinMovementRange { get; private set; }

        public HashSet<GameBoardCell>? CellsWithinAttackRange { get; private set; }

        public UnitToken? SelectedToken => SelectedCell?.UnitToken;

        public Vector2I TileSize { get; private set; }

        public Vector2I TileSizeHalf { get; private set; }

        public Vector2 TileSizeStacked { get; private set; }

        public override void _Ready()
        {
            TileSize = TileSet.TileSize;
            TileSizeHalf = TileSize / 2;
            TileSizeStacked = new Vector2(TileSize.X * 0.75f, TileSize.Y);

            base._Ready();

            InitializeData();

            LateReadyAsync();
        }

        public async void LateReadyAsync()
        {
            await Task.Yield();

            var faction = "player";
            var unitTokenScenePath = "res://Frontend/UI/Controls/GameBoard/UnitTokens/ClaireUnitTokenScene.tscn";

            SpawnToken(Cells[new(1, 3)], unitTokenScenePath, faction);
        }

        protected override void Dispose(bool disposing)
        {
            EmitSignal(SignalName.Disposing, this);

            base.Dispose(disposing);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var localMousePos = GetLocalMousePosition();
            var cellCoords = this.GetCellFromLocal(localMousePos);

            CellHover(cellCoords);
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

                    CellPrimaryTouch(mapSpaceClickLocation);
                }

                if (mouseevent.ButtonIndex == MouseButton.Right && mouseevent.Pressed)
                {
                    var screenSpaceClickLocation = GetLocalMousePosition();

                    var mapSpaceClickLocation = LocalToMap(screenSpaceClickLocation);

                    Debugger.Log(2, "Info", $"{mapSpaceClickLocation}\n");

                    CellSecondaryTouch(mapSpaceClickLocation);
                }
            }
        }

        private void ClearChildren()
        {
            var children = FindChildren("*");

            foreach (var child in children)
            {
                child.QueueFree();
            }
        }

        private void InitializeData()
        {
            ClearChildren();

            var usedCells = GetUsedCells(GROUND_LAYER_INDEX);

            Cells.Clear();

            foreach (var cell in usedCells)
            {
                Cells.Add(cell, new(this, cell));

                var localPos = this.GetLocalFromCell(cell);

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
        }

        public void SetCellStyle(Vector2I cell, Styles style)
        {
            var styleIndex = StylesIndex[style];

            SetCell(
                GROUND_LAYER_INDEX,
                cell,
                sourceId: DEFAULT_TILESET_ID,
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

        public void CellPrimaryTouch(Vector2I cellCoordinates)
        {
            if (!Cells.TryGetValue(cellCoordinates, out var c))
                throw new KeyNotFoundException(cellCoordinates.ToString());

            c.OnPrimaryTouch();
        }

        public void CellSecondaryTouch(Vector2I cellCoordinates)
        {
            if (!Cells.TryGetValue(cellCoordinates, out var c))
                throw new KeyNotFoundException(cellCoordinates.ToString());

            c.OnSecondaryTouch();
        }

        public void OnCellMouseOver(Vector2I cellCoordinates)
        {
            if (!Cells.TryGetValue(cellCoordinates, out var c))
                throw new KeyNotFoundException(cellCoordinates.ToString());

            c.OnHover();
        }

        public void CreateCells(IEnumerable<Vector2I> cells)
        {
            Cells = new(cells.Count());

            foreach (var cell in cells)
            {
                Cells[cell] = new GameBoardCell(this, cell);
            }
        }

        public void SelectCell(GameBoardCell cell)
        {
            ClearSelectedCell();

            SelectedCell = cell;
            cell.IsSelected = true;

            SetCellStyle(cell.BoardPosition, Styles.Selected);

            EmitSignal(SignalName.CellSelected, cell.BoardPosition);

            if (SelectedToken is not null)
            {
                EmitSignal(SignalName.UnitTokenSelected, SelectedToken);

                ShowTokenMovementRange(SelectedToken);
            }
        }

        public void ClearSelectedCell()
        {
            if (SelectedCell is null)
                return;

            var cell = SelectedCell.BoardPosition;
            var selectedToken = SelectedToken;

            SelectedCell.IsSelected = false;
            SelectedCell = null;

            RemoveCellStyles(cell, Styles.Selected);

            EmitSignal(SignalName.CellUnselected, cell);

            if (selectedToken is not null)
            {
                EmitSignal(SignalName.UnitTokenUnselected, selectedToken);

                ClearTokenMovementRange();
            }
        }

        public void ClearHoveredCell()
        {
            if(HoveredCell is null)
                return;

            HoveredCell.IsHovered = false;
            var cell = HoveredCell;

            HoveredCell = null;

            RemoveCellStyles(cell.BoardPosition, Styles.Hovered);

            EmitSignal(SignalName.CellUnhovered, cell.BoardPosition);
        }

        public void CellHover(Vector2I cell)
        {
            ClearHoveredCell();

            if (!Cells.TryGetValue(cell, out var gameCell))
                return;

            gameCell.IsHovered = true;
            HoveredCell = gameCell;

            SetCellStyle(cell, Styles.Hovered);

            EmitSignal(SignalName.CellHovered, cell);
        }

        public void HoverCell(GameBoardCell cell)
        {
            ClearHoveredCell();

            cell.IsHovered = true;
            HoveredCell = cell;

            SetCellStyle(cell.BoardPosition, Styles.Hovered);

            EmitSignal(SignalName.CellHovered, cell.BoardPosition);
        }

        public void MoveToken(UnitToken token, GameBoardCell from, GameBoardCell to)
        {
            if (token.BoardPieceMovement is null)
                return;

            if(!token.BoardPieceMovement.MoveTo(to.BoardPosition))
                return;

            token.Position = this.GetLocalFromCell(to.BoardPosition);

            from.RemoveToken();

            to.SetToken(token);

            _tokenCells[token] = to;

            EmitSignal(SignalName.UnitTokenMoved, token, from.BoardPosition, to.BoardPosition);

            ClearTokenMovementRange();
        }

        public UnitToken? SpawnToken(GameBoardCell cell, string unitScenePath, string faction)
        {
            if (cell.UnitToken is not null)
                return null;

            var packedScene = GD.Load<PackedScene>(unitScenePath);
            var token = packedScene.Instantiate<UnitToken>();

            token.Faction = faction;

            token.Position = this.GetLocalFromCell(cell.BoardPosition);
            token.ZIndex = 1;

            void TokenReady()
            {
                if (token.BoardPieceHealth is not null)
                {
                    token.BoardPieceHealth.HealthChanged += Token_HealthChanged;
                    //token.BoardPieceHealth.HealthAtMin += Token_HealthReachedZero;
                }

                if (token.BoardPieceMovement is not null)
                {
                    token.BoardPieceMovement.SetAvailableCells(GetUsedCells(GROUND_LAYER_INDEX));

                    token.BoardPieceMovement.ForcePosition(cell.BoardPosition);
                }
            }

            void TokenDisposing(Node sender)
            {
                if (sender is not UnitToken token)
                    return;

                token.Disposing -= TokenDisposing;
                token.Ready -= TokenReady;
                //token.HealthChanged -= Token_HealthChanged;
                //token.HealthReachedZero -= Token_HealthReachedZero;

                // Remove from the master list.
                _tokens.Remove(token);

                // Remove from the cell's items.
                _tokenCells[token].RemoveToken();

                EmitSignal(SignalName.UnitTokenDisposing, token);

                token.Free();
            }

            token.Disposing += TokenDisposing;
            token.Ready += TokenReady;

            cell.SetToken(token);
            _tokens.Add(token);
            _tokenCells.Add(token, cell);

            EmitSignal(SignalName.UnitTokenCreated, token, cell.BoardPosition);



            var sprite = token.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            if (sprite is null)
                throw new Exception("Could not find sprite");

            sprite.Play();



            AddChild(token);

            return token;
        }

        private void Token_HealthChanged(BoardPieceHealth sender, float previousHealth, float newHealth)
        {
            EmitSignal(SignalName.UnitTokenHealthChanged, sender, previousHealth, newHealth);

            var damageLabel = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"{newHealth - previousHealth:n0}",
                Position = new Vector2(20, -30),
            };
            damageLabel.AddThemeColorOverride("font_color", Colors.Red);
            damageLabel.AddThemeFontSizeOverride("font_size", 30);
            GetTree().Root.AddChild(damageLabel);

            var tween = damageLabel.CreateTween();
            tween.TweenProperty(damageLabel, "position", damageLabel.Position + new Vector2(0, -40), 1f).SetTrans(Tween.TransitionType.Expo);

            var timer = GetTree().CreateTimer(1);
            timer.Timeout += () => damageLabel.QueueFree();
        }

        private void Token_HealthReachedZero(UnitToken token)
        {
            EmitSignal(SignalName.UnitTokenHealthReachedZero, token);

            token.Dispose();
        }

        public void ShowTokenMovementRange(UnitToken token)
        {
            if (token.BoardPieceMovement is null)
                return;

            if (!_tokenCells.TryGetValue(token, out var tokenCell))
                return;

            var range = token.BoardPieceMovement.MaximumMoveRange;
            var movementCells = token.BoardPieceMovement.CellsInRange
                .Select(f => Cells[f])
                .Where(f => f != SelectedCell)
                .ToHashSet();

            if (movementCells.Count == 0)
            {
                CellsWithinMovementRange?.Clear();

                return;
            }

            CellsWithinMovementRange = movementCells;

            foreach (var cell in movementCells)
            {
                cell.IsInMovementRange = true;

                SetCellStyle(cell.BoardPosition, Styles.InRange);
            }

            EmitSignal(SignalName.ShowUnitTokenMovementRange,
                new Godot.Collections.Array<Vector2I>(movementCells.Select(f => f.BoardPosition)));
        }

        public void ClearTokenMovementRange()
        {
            if (CellsWithinMovementRange is null || CellsWithinMovementRange.Count == 0)
                return;

            foreach (var cell in CellsWithinMovementRange)
            {
                cell.IsInMovementRange = false;

                RemoveCellStyles(cell.BoardPosition, Styles.InRange);
            }

            EmitSignal(SignalName.ClearUnitTokenMovementRange,
                new Godot.Collections.Array<Vector2I>(CellsWithinMovementRange.Select(f => f.BoardPosition)));

            CellsWithinMovementRange?.Clear();
        }
    }
}
