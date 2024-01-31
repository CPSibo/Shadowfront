using Godot;
using Shadowfront.Backend.Board.BoardPieces;
using Shadowfront.Backend.Board.BoardPieces.Behaviors;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Movement;
using Shadowfront.Backend.Utilities;
using Shadowfront.Frontend.UI.Subscreens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Shadowfront.Backend.Board
{
    public partial class GameBoard : TileMap, IDisposableNode
    {
        public static GameBoard Instance { get; private set; } = null!;

        [Export]
        public CellOutliner? CellOutliner { get; set; }

        public int GROUND_LAYER_INDEX = 0;

        public Dictionary<Vector2I, GameBoardCell> Cells { get; private set; } = [];

        private readonly List<BoardPiece> _boardPieces = [];

        private readonly Dictionary<BoardPiece, GameBoardCell> _boardPieceCells = [];

        public GameBoardCell? ActiveCell { get; private set; }

        public BoardPiece? ActiveBoardPiece => ActiveCell?.BoardPiece;

        public Vector2I TileSize { get; private set; }

        public Vector2I TileSizeHalf { get; private set; }

        public Vector2 TileSizeStacked { get; private set; }

        private BoardPieceInteraction? _activeInteraction = null;

        private readonly Color HOVER_COLOR = new(0x00000055);

        private readonly Color ACTIVE_COLOR = new(0xffce9555);

        private const string ACTIVE_KEY = "active";

        private const string HOVER_KEY = "hover";

        public GameBoard()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            TileSize = TileSet.TileSize;
            TileSizeHalf = TileSize / 2;
            TileSizeStacked = new Vector2(TileSize.X * 0.75f, TileSize.Y);

            base._Ready();

            EventBus.Subscribe<BoardPieceActionBar_InteractionButtonClickedEvent>(BoardPieceActionBar_InteractionButtonClicked);

            EventBus.Subscribe<BoardPiece_DisposingEvent>(BoardPiece_Disposing);
            EventBus.Subscribe<BoardPiece_HealthAtMinEvent>(BoardPiece_HealthAtMin);

            EventBus.Subscribe<BoardPieceMovement_PositionChangedEvent>(BoardPieceMovement_PositionChanged);

            EventBus.Subscribe<GameBoard_BoardPieceCreationRequestedEvent>(BoardPieceCreationRequested);

            InitializeData();

            LateReadyAsync();
        }

        public async void LateReadyAsync()
        {
            await Task.Yield();

            var faction = "player";
            var boardPieceScenePath = "res://Frontend/UI/Controls/GameBoard/UnitTokens/ClaireUnitTokenScene.tscn";

            CreateBoardPiece(Cells[new(1, 3)], boardPieceScenePath, faction);

            CreateBoardPiece(Cells[new(19, 0)], boardPieceScenePath, faction);
            CreateBoardPiece(Cells[new(20, 1)], boardPieceScenePath, faction);

            CreateBoardPiece(Cells[new(1, 0)], boardPieceScenePath, "enemy");
        }

        protected override void Dispose(bool disposing)
        {
            EventBus.Unsubscribe<BoardPieceActionBar_InteractionButtonClickedEvent>(BoardPieceActionBar_InteractionButtonClicked);

            EventBus.Unsubscribe<BoardPiece_DisposingEvent>(BoardPiece_Disposing);
            EventBus.Unsubscribe<BoardPiece_HealthAtMinEvent>(BoardPiece_HealthAtMin);

            EventBus.Unsubscribe<BoardPieceMovement_PositionChangedEvent>(BoardPieceMovement_PositionChanged);

            EventBus.Unsubscribe<GameBoard_BoardPieceCreationRequestedEvent>(BoardPieceCreationRequested);

            EventBus.Emit(new GameBoard_DisposingEvent(this));

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

                    HandleCellPrimaryTouched(mapSpaceClickLocation);
                }

                if (mouseevent.ButtonIndex == MouseButton.Right && mouseevent.Pressed)
                {
                    var screenSpaceClickLocation = GetLocalMousePosition();

                    var mapSpaceClickLocation = LocalToMap(screenSpaceClickLocation);

                    Debugger.Log(2, "Info", $"{mapSpaceClickLocation}\n");

                    HandleCellSecondaryTouched(mapSpaceClickLocation);
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

            var count = 0;

            foreach (var cell in usedCells)
            {
                Cells.Add(cell, new(cell, count));

                count++;

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

            EventBus.Emit(new GameBoard_GroundCellsChanged(Cells.Values));

            //var navGraph = GenerateNavigationGraph();

            //var startNode = Cells[new(1, 3)];
            //var endNode = Cells[new(-2, 1)];

            //var path = navGraph.GetPointPath(startNode.Id, endNode.Id);

            //if (path is null || path.Length == 0)
            //    Debugger.Log(0, "INFO", "No path found\n");
            //else
            //{
            //    Debugger.Log(0, "INFO", $"{string.Join(" -> ", path)}\n");
            //}
        }

        private AStar2D GenerateNavigationGraph()
        {
            var aStar = new AStar2D();
            aStar.ReserveSpace(Cells.Count);

            foreach (var cell in Cells)
            {
                aStar.AddPoint(cell.Value.Id, cell.Key);
            }

            foreach (var (position, node) in Cells)
            {
                var hypotheticalNeighbors = HexTileMapUtils.GetHypotheticalCellsWithinRange(position, 0, 1);

                foreach (var hypotheticalNeighbor in hypotheticalNeighbors)
                {
                    if (!Cells.TryGetValue(hypotheticalNeighbor, out var realCell))
                        continue;

                    aStar.ConnectPoints(node.Id, realCell.Id);
                }
            }

            return aStar;
        }

        public void HandleCellPrimaryTouched(Vector2I cellCoordinates)
        {
            // Get the cell that was interacted with.
            if (!Cells.TryGetValue(cellCoordinates, out var cell))
                return;

            // If we interacted with the currently-active cell...
            if (ActiveCell == cell)
            {
                // Deactivate the cell.
                DeactivateCell();

                return;
            }

            // If we interacted with a cell that has a board piece...
            if (cell.BoardPiece?.Faction == "player")
            {
                // Activate the cell.
                ActivateCell(cell);

                return;
            }

            if (_activeInteraction is null)
            {
                // Deactivate the cell.
                DeactivateCell();

                return;
            }

            // If we already have an active board piece and clicked somewhere else... 
            if (ActiveCell is not null && ActiveBoardPiece is not null)
            {
                var activeCell = ActiveCell;
                var activeBoardPiece = ActiveBoardPiece;
                var activeInteraction = _activeInteraction;

                // Deactivate everything.
                DeactivateCell();

                activeInteraction.Perform(new(
                    ActingCell: activeCell,
                    ActingPiece: activeBoardPiece,
                    TargetPiece: cell.BoardPiece,
                    TargetCell: cell
                ));

                return;
            }

            // Create a new board piece in the cell we interacted with. 

            var faction = "player"; // DevWindow.Instance.SelectedFaction;
            var unitTokenScenePath = "res://Frontend/UI/Controls/GameBoard/UnitTokens/ClaireUnitTokenScene.tscn"; // DevWindow.Instance.SelectedTokenType;

            if (faction is null)
                throw new Exception("No faction given");

            if (unitTokenScenePath is null)
                throw new Exception("No token type given");

            CreateBoardPiece(
                cell,
                unitTokenScenePath,
                faction
            );
        }

        public void HandleCellSecondaryTouched(Vector2I cellCoordinates)
        {
            if (!Cells.TryGetValue(cellCoordinates, out var cell))
                throw new KeyNotFoundException(cellCoordinates.ToString());

            if (ActiveCell == cell)
                return; // NOOP

            // NOOP
        }

        public void ActivateCell(GameBoardCell cell)
        {
            DeactivateCell();

            ActiveCell = cell;

            CellOutliner?.AddCellStyle(ACTIVE_KEY, MapToLocal(cell.BoardPosition), ACTIVE_COLOR);

            if (ActiveBoardPiece is not null)
            {
                EventBus.Emit(new GameBoard_BoardPieceActivatedEvent(ActiveBoardPiece));
            }
        }

        public void DeactivateCell()
        {
            if (_activeInteraction is not null)
                CellOutliner?.RemoveSource(_activeInteraction.GetType().Name);

            _activeInteraction = null;

            if (ActiveCell is null)
                return;

            var cell = ActiveCell.BoardPosition;
            var activeBoardPiece = ActiveBoardPiece;

            ActiveCell = null;

            CellOutliner?.RemoveSource(ACTIVE_KEY);

            if (activeBoardPiece is not null)
            {
                EventBus.Emit(new GameBoard_BoardPieceDectivatedEvent(activeBoardPiece));
            }
        }

        public void CellHover(Vector2I cell)
        {
            ClearHoveredCell();

            if (!Cells.TryGetValue(cell, out var gameCell))
                return;

            CellOutliner?.AddCellStyle(HOVER_KEY, MapToLocal(gameCell.BoardPosition), HOVER_COLOR);
        }

        public void ClearHoveredCell()
        {
            CellOutliner?.RemoveSource(HOVER_KEY);
        }

        private void BoardPieceMovement_PositionChanged(BoardPieceMovement_PositionChangedEvent e)
        {
            if (Cells.TryGetValue(e.PreviousPosition, out var from))
                from.RemoveBoardPiece();

            if (Cells.TryGetValue(e.NewPosition, out var to))
            {
                // Set the visible node position on screen.
                e.BoardPiece.Position = this.GetLocalFromCell(to.BoardPosition);

                to.SetBoardPiece(e.BoardPiece);

                _boardPieceCells[e.BoardPiece] = to;
            }
        }

        public BoardPiece? CreateBoardPiece(GameBoardCell cell, string boardPieceScenePath, string faction)
        {
            if (cell.BoardPiece is not null)
                return null;

            var packedScene = GD.Load<PackedScene>(boardPieceScenePath);
            var boardPiece = packedScene.Instantiate<BoardPiece>();

            boardPiece.Faction = faction;

            boardPiece.Position = this.GetLocalFromCell(cell.BoardPosition);
            boardPiece.ZIndex = 1;

            void BoardPieceReady()
            {
                boardPiece.Ready -= BoardPieceReady;

                boardPiece.BoardPieceMovement?.ForcePosition(cell.BoardPosition);
            }

            boardPiece.Ready += BoardPieceReady;

            cell.SetBoardPiece(boardPiece);
            _boardPieces.Add(boardPiece);
            _boardPieceCells.Add(boardPiece, cell);

            EventBus.Emit(new GameBoard_BoardPiecePlacedEvent(boardPiece, cell.BoardPosition));



            var sprite = boardPiece.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            if (sprite is null)
                throw new Exception("Could not find sprite");

            sprite.Play();



            AddChild(boardPiece);

            return boardPiece;
        }

        private void BoardPieceCreationRequested(GameBoard_BoardPieceCreationRequestedEvent e)
        {
            var location = e.Position;
            var cell = Cells[location];

            CreateBoardPiece(
                boardPieceScenePath: e.BoardPieceScenePath,
                cell: cell,
                faction: e.Faction
            );
        }

        private void BoardPiece_HealthAtMin(BoardPiece_HealthAtMinEvent e)
        {
            e.BoardPiece.Dispose();
        }

        private void BoardPiece_Disposing(BoardPiece_DisposingEvent e)
        {
            // Remove from the master list.
            _boardPieces.Remove(e.BoardPiece);

            e.BoardPiece.QueueFree();
        }

        public void BoardPieceActionBar_InteractionButtonClicked(BoardPieceActionBar_InteractionButtonClickedEvent e)
        {
            if (ActiveBoardPiece is null)
                return;

            if (_activeInteraction is not null)
                CellOutliner?.RemoveSource(_activeInteraction.GetType().Name);

            _activeInteraction = e.Interaction;

            if (_activeInteraction is IHasRange rangedInteraction)
            {
                CellOutliner?.AddCellStyle(
                    rangedInteraction.GetType().Name,
                    rangedInteraction.ValidCellsInRange.Select(MapToLocal),
                    rangedInteraction.RangeColor);

                CellOutliner?.AddCellStyle(
                    rangedInteraction.GetType().Name,
                    rangedInteraction.CellsInRange.Select(MapToLocal),
                    new(0x00000060));
            }
        }
    }

    public readonly record struct GameBoard_DisposingEvent(GameBoard GameBoard) : IEventType;

    public readonly record struct GameBoard_BoardPieceActivatedEvent(BoardPiece BoardPiece) : IEventType;

    public readonly record struct GameBoard_BoardPieceDectivatedEvent(BoardPiece BoardPiece) : IEventType;

    public readonly record struct GameBoard_BoardPieceCreationRequestedEvent(string BoardPieceScenePath, string Faction, Vector2I Position) : IEventType;

    public readonly record struct GameBoard_BoardPiecePlacedEvent(BoardPiece BoardPiece, Vector2I Position) : IEventType;

    public readonly record struct GameBoard_GroundCellsChanged(IEnumerable<GameBoardCell> Cells) : IEventType;
}
