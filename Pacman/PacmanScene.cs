using GAlgoT2530.Engine;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace PacmanGame
{
    public class PacmanScene : GameScene
    {
        private GameMap _gameMap;
        private Ghost _ghost;
        private Pacman _pacman;
        private Tile _goalTile;
        private ScoreUI _scoreUI;
        private SoundEffect LoseSound, WinSound;
        private float _gameOverDelaySeconds;
        private float _gameOverTimer;

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

            LoseSound = _game.Content.Load<SoundEffect>("LoseSound");
            WinSound = _game.Content.Load<SoundEffect>("WinSound");

            _gameOverDelaySeconds = 5f;
            _gameOverTimer = _gameOverDelaySeconds;
        }

        
        // Updating the map, replaces the power pellet tile with tthe empty tile.
        public override void Update()
        {
            _gameMap.Update();
            if (!_pacman.isGameOver)
            {
                if (_pacman._currTile.Equals(_gameMap.goalTile))
                {
                    _pacman.isGameOver = true;
                    WinSound.Play();
                    _scoreUI.winScore++;
                    _scoreUI.CurrentGameState = ScoreUI.GameState.Win;
                }
                if (_pacman._currTile.Equals(Tile.ToTile(_ghost.Position, _gameMap.TiledMap.TileWidth, _gameMap.TiledMap.TileHeight)))
                {
                    _pacman.isGameOver = true;
                    LoseSound.Play();
                    _scoreUI.loseScore++;
                    _scoreUI.CurrentGameState = ScoreUI.GameState.Lose;
                }
            }
            else
            {
                if (_gameOverTimer < 0)
                {
                    _gameOverTimer = _gameOverDelaySeconds;
                    RestartScene();
                }
                _gameOverTimer -= ScalableGameTime.DeltaTime;
            }
            
        }

        public void RestartScene()
        {
            _ghost.Initialize();
            _pacman.Initialize();
            _scoreUI.CurrentGameState = ScoreUI.GameState.Playing;
        }
        
    }
}
