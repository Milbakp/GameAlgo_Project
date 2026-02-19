using GAlgoT2530.Engine;
using System.Diagnostics;

namespace PacmanGame
{
    public class PacmanScene : GameScene
    {
        private GameMap _gameMap;
        private Ghost _ghost;
        private Pacman _pacman;
        private Tile _goalTile;
        public override void CreateScene()
        {
            // Game map
            GameMap gameMap = new GameMap("GameMap");
            // No need to manually set home tile here anymore, edits in GameMap.cs automatically set it.
            // gameMap.StartColumn = 0;
            // gameMap.StartRow = 8;
            _gameMap = gameMap;
            _goalTile = _gameMap.goalTile;

            // Pathfinding Tester
            // PathfindingTester pathfindingTester = new PathfindingTester("PathfindingTester");

            // Ghost
            _ghost = new Ghost();

            _pacman = new Pacman();
            _pacman.Speed = 100.0f;
            // _pacman.StartRow = _gameMap.StartRow;
            // _pacman.StartColumn = _gameMap.StartColumn;
            _pacman.NavigableTileLayerName = "Food";



            // Trip Tile Manager
            TripTileManager tripTileManager = new TripTileManager();

        }
        // Updating the map, replaces the power pellet tile with tthe empty tile.
        public override void Update()
        {
            _gameMap.Update();
            if (_pacman._currTile.Equals(_goalTile))
            {
                RestartScene();
            }
        }

        public void RestartScene()
        {
            // Initialize positions
            // Tile _startTile = new Tile(_pacman.StartColumn, _pacman.StartRow);
            // _pacman.Position = Tile.ToPosition(_startTile, _gameMap.TiledMap.TileWidth, _gameMap.TiledMap.TileHeight);
            // // _pacman._nextTilePosition = _pacman.Position;
            // Tile startTile = new Tile(_gameMap.StartColumn, _gameMap.StartRow);
            // _ghost.Position = Tile.ToPosition(startTile, _gameMap.TiledMap.TileWidth, _gameMap.TiledMap.TileHeight);
            _ghost.Initialize();
            _pacman.Initialize();
        }
    }
}
