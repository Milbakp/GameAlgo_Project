// Based from the original Pacman.cs
using GAlgoT2530.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Content;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace PacmanGame
{
    public class Pacman : AnimationGameObject // GameObject
    {
        // Delegates
        public delegate void TileReachedHandler(Tile location);
        // Events
        public event TileReachedHandler TileReached;

        // Attributes
        public float Speed;
        public int StartColumn;
        public int StartRow;
        public string NavigableTileLayerName;

        private enum Direction { UpLeft, Up, UpRight, Left, None, Right, DownLeft, Down, DownRight };

        private readonly int[] NextRow = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
        private readonly int[] NextCol = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };

        private Direction _currDirection;
        private Direction _prevDirection; // not in use anymore

        public Tile _currTile;
        private Vector2 _nextTilePosition;

        private TiledMap _tiledMap;
        private TiledMapTileLayer _tiledMapNavigableLayer;
        // New Project variables
        public Dictionary<(int x, int y), float> HeatMap = new Dictionary<(int x, int y), float>();
        private TripTileManager _tripTileManager;

        public Pacman() : base("Pacman", "pacman-animations.sf")
        {
        }

        public override void Initialize()
        {
            // Initialize directions
            _currDirection = Direction.None;
            _prevDirection = Direction.None;

            // Initialize animations
            AnimatedSprite.SetAnimation("pacmanCentre");
            AnimatedSprite.TextureRegion = SpriteSheet.TextureAtlas[AnimatedSprite.Controller.CurrentFrame];

            // Get graph and tiled map
            GameMap gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");
            _tiledMap = gameMap.TiledMap;
            _tiledMapNavigableLayer = _tiledMap.GetLayer<TiledMapTileLayer>(NavigableTileLayerName);

            // Initialize positions
            _currTile = new Tile(gameMap.StartColumn, gameMap.StartRow);
            Position = Tile.ToPosition(_currTile, _tiledMap.TileWidth, _tiledMap.TileHeight);
            _nextTilePosition = Position;

            // Fills the heatmap with all the navigable tiles in the map only on the first run.
            if(HeatMap.Count == 0)
            {
               foreach (TiledMapTile? tile in _tiledMapNavigableLayer.Tiles)
                {
                    if (tile.HasValue)
                    {
                        if (!tile.Value.IsBlank)
                        {
                            HeatMap.Add((tile.Value.X, tile.Value.Y), 0);
                        }
                    }
                } 
            }
            _tripTileManager = (TripTileManager)GameObjectCollection.FindByName("TripTileManager");
        }

        public override void Update()
        {
            // Update direction from user input
            Direction newDirection = GetDirectionFromInput();
            UpdateDirection(newDirection);

            // Calculate a new next tile and position when Pacman reach its old next tile
            if (Position.Equals(_nextTilePosition))
            {
                _currTile = Tile.ToTile(Position, _tiledMap.TileWidth, _tiledMap.TileHeight);

                // Call the reach tile callback
                TileReached?.Invoke(_currTile);

                // To calculate the next tile, first assume that Pacman does not move.
                Tile nextTile = _currTile;

                nextTile = GetNextTileFromDirection(_currDirection);
                ushort col = (ushort)nextTile.Col;
                ushort row = (ushort)nextTile.Row;

                if (_tiledMapNavigableLayer.TryGetTile(col, row, out TiledMapTile? nextTiledMapTile)|| 
                    (_tripTileManager.blockedTiles.ContainsKey(nextTile) && _tripTileManager.blockedTiles[nextTile]))
                {
                        // BLANK: Pacman/Player found the next tile non-navigable
                        if (nextTiledMapTile.Value.IsBlank)
                        {
                            nextTile = _currTile;
                        }
                }

                // Update animation
                UpdateAnimatedSprite(_currTile, nextTile);

                // Recalculate next position to go to only if Pacman's next tile is a different tile
                if (!nextTile.Equals(_currTile))
                {
                    _nextTilePosition = Tile.ToPosition(nextTile, _tiledMap.TileWidth, _tiledMap.TileHeight);
                    if(HeatMap != null && HeatMap.ContainsKey((nextTile.Col, nextTile.Row)))
                    {
                        HeatMap[(nextTile.Col, nextTile.Row)] += 1;
                        Debug.WriteLine($"HeatMap value: {HeatMap[(nextTile.Col, nextTile.Row)]}");
                    }
                }
                else
                {
                    // Reset current and previous direction due to no movement.
                    _currDirection = Direction.None;
                    _prevDirection = Direction.None;
                }
                
            }

            // Move and animate pacman towards the next tile's position
            Position = Move(Position, _nextTilePosition, ScalableGameTime.DeltaTime, Speed);
            AnimatedSprite.Update(ScalableGameTime.GameTime);
        }

        public override void Draw()
        {
            _game.SpriteBatch.Begin();
            _game.SpriteBatch.Draw(AnimatedSprite, Position, Orientation, Scale);
            _game.SpriteBatch.End();
        }

        private Direction GetDirectionFromInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W))
            {
                return Direction.Up;
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                return Direction.Down;
            }
            else if (keyboardState.IsKeyDown(Keys.A))
            {
                return Direction.Left;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                return Direction.Right;
            }
            else
            {
                return Direction.None;
            }
        }
        // Altered since in this game, the player should be able to change direction immediately and stop moving in the middle of the road.
        private void UpdateDirection(Direction newDirection)
        {
            // "newDirection = Direction.None" means no use input is entered
            if (newDirection != Direction.None)
            {
                _currDirection = newDirection;
            }
        }

        private Tile GetNextTileFromDirection(Direction direction)
        {
            int directionIndex = (int)direction;

            Tile nextTile = new Tile(_currTile.Col + NextCol[directionIndex],
                                        _currTile.Row + NextRow[directionIndex]);

            return nextTile;
        }

        // Given source (src) and destination (dest) locations, and elapsed time, 
        //     try to move from source to destination at the given speed within elapsed time.
        // If cannot reach dest within the elapsed time, return the location where it will reach
        // If can reach or over-reach the dest, the return dest.
        public Vector2 Move(Vector2 src, Vector2 dest, float elapsedSeconds, float speed)
        {
            Vector2 dP = dest - src;
            float distance = dP.Length();
            float step = speed * elapsedSeconds;

            if (step < distance)
            {
                dP.Normalize();
                return src + (dP * step);
            }
            else
            {
                return dest;
            }
        }

        // Select the Pacman's animation based on:
        // (a) Which tile the Pacman is standing on (currTile)
        // (b) Which tile the Pacman is heading next (nextTile)
        //
        // Pre-conditions:
        //    The animation name is suffixed by:
        //      "UpLeft", "Up", "UpRight", "Left", "Stop", "Right", "Downleft", "Down", "DownRight"
        //
        // Example:
        //    If nextTile is on the RIGHT of currTile, the animation to play is "pacmanRight".
        public void UpdateAnimatedSprite(Tile currTile, Tile nextTile)
        {
            string[] directions = {"UpLeft", "Up", "UpRight",
                                   "Left", "Centre", "Right",
                                   "Downleft", "Down", "DownRight"};

            if (currTile == null || nextTile == null)
            {
                throw new ArgumentNullException("UpdateAnimatedSprite(): NULL in current tile or next tile.");
            }
            else
            {
                Tile difference = new Tile(nextTile.Col - currTile.Col, nextTile.Row - currTile.Row);
                int index = (difference.Col + 1) + 3 * (difference.Row + 1);

                string animationName = $"pacman{directions[index]}";

                if (AnimatedSprite.CurrentAnimation != animationName)
                {
                    AnimatedSprite.SetAnimation(animationName);
                    AnimatedSprite.TextureRegion = SpriteSheet.TextureAtlas[AnimatedSprite.Controller.CurrentFrame];
                }
            }
        }
    }
}
