using GAlgoT2530.Engine;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PacmanGame
{
    public class PacmanScene : GameScene
    {
        private GameMap _gameMap;
        private Ghost _ghost;
        private Pacman _pacman;
        private Tile _goalTile;
        private ScoreUI _scoreUI;

        public override void CreateScene()
        {
            // Game map
            GameMap gameMap = new GameMap("GameMap");
            // No need to manually set home tile here anymore, edits in GameMap.cs automatically set it.
            // gameMap.StartColumn = 0;
            // gameMap.StartRow = 8;
            _gameMap = gameMap;

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

            _scoreUI = new ScoreUI();
            _scoreUI.Initialize();
        }

        
        // Updating the map, replaces the power pellet tile with tthe empty tile.
        public override void Update()
        {
            _gameMap.Update();
            if (_pacman._currTile.Equals(_gameMap.goalTile))
            {
                _scoreUI.winScore++;
                RestartScene();
            }
            if (_pacman._currTile.Equals(Tile.ToTile(_ghost.Position, _gameMap.TiledMap.TileWidth, _gameMap.TiledMap.TileHeight)))
            {
                _scoreUI.loseScore++;
                RestartScene();
            }
        }

        public void RestartScene()
        {
            _ghost.Initialize();
            _pacman.Initialize();
        }
        
    }
}
