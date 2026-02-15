using GAlgoT2530.Engine;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using System.Collections.Generic;
using System.Diagnostics;
using MonoGame.Extended.Graphics;
using System;
using GAlgoT2530.IO;
using GAlgoT2530.AI;

namespace PacmanGame
{
    public class NavigationHCFSM : HCFSM
    {
        public enum NavigationState { STOP, MOVING };

        // Navigation current state
        private NavigationState _currentState = NavigationState.STOP;

        // Navigation
        private Tile _srcTile;
        private Tile _destTile;
        private LinkedList<Tile> _path;
        private Ghost _ghost;

        private Rectangle _ghostRect;
        private TiledMap _tiledMap;
        private TileGraph _tileGraph;

        public NavigationHCFSM(Ghost ghost, NavigationState currentState)
        {
            _ghost = ghost;
            _currentState = currentState;
        }

        public override void Initialize()
        {
            _ghost.MaxSpeed = 100.0f;

            GameMap gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");
            _tiledMap = gameMap.TiledMap;
            _tileGraph = gameMap.TileGraph;

            // Initialize Animation to "ghostRedDown".
            _ghost.AnimatedSprite.SetAnimation("ghostRedDown");
            _ghost.AnimatedSprite.TextureRegion = _ghost.SpriteSheet.TextureAtlas[_ghost.AnimatedSprite.Controller.CurrentFrame];

            // Initialize Source Tile
            _srcTile = new Tile(gameMap.StartColumn, gameMap.StartRow);

            // Initialize Position
            _ghost.Position = Tile.ToPosition(_srcTile, _tiledMap.TileWidth, _tiledMap.TileHeight);
        }
        public override void Update()
        {
            MouseState mouse = Mouse.GetState();

            int tileWidth = _tiledMap.TileWidth;
            int tileHeight = _tiledMap.TileHeight;

            // Implement the movement behaviour
            if (_currentState == NavigationState.STOP)
            {
                // Left mouse button pressed
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    // Get destination tile as the mouse-selected tile
                    _destTile = Tile.ToTile(mouse.Position.ToVector2(), tileWidth, tileHeight);

                    if (_tileGraph.Nodes.Contains(_destTile) &&
                        !_destTile.Equals(_srcTile)
                       )
                    {
                        // Transition Actions
                        // 1. Compute an A* path
                        _path = AStar.Compute(_tileGraph, _srcTile, _destTile, AStarHeuristic.EuclideanSquared);
                        // 2. Remove the source tile from the path
                        _path.RemoveFirst();

                        _ghost.UpdateAnimatedSprite(_srcTile, _path.First.Value);


                        // Change to MOVING state
                        _currentState = NavigationState.MOVING;
                    }

                    // NOTE: No action to execute for STOP state
                }
            }
            else if (_currentState == NavigationState.MOVING)
            {
                float elapsedSeconds = ScalableGameTime.DeltaTime;

                if (_path.Count == 0 ||
                    _ghost.Position.Equals(Tile.ToPosition(_destTile, tileWidth, tileHeight))
                   )
                {
                    // Update source tile to destination tile
                    _srcTile = _destTile;
                    _destTile = null;

                    // Change to STOP state
                    _currentState = NavigationState.STOP;
                }

                // Action to execute on the MOVING state
                else
                {
                    Tile nextTile = _path.First.Value; // throw exception if path is empty

                    Vector2 nextTilePosition = Tile.ToPosition(nextTile, tileWidth, tileHeight);

                    if (_ghost.Position.Equals(nextTilePosition))
                    {
                        Debug.WriteLine($"Reached the next tile (Col = {nextTile.Col}, Row = {nextTile.Row}).");
                        Debug.WriteLine($"Removing this tile from the path and getting the new next tile from path.");
                        
                        // Get the position of the new next tile from the path
                        _path.RemoveFirst();
                        Tile newNextTile = _path.First.Value;
                        nextTilePosition = Tile.ToPosition(newNextTile, tileWidth, tileHeight);

                        // Update the animation
                        _ghost.UpdateAnimatedSprite(nextTile, newNextTile);
                        

                    }

                    // Move the ghost to the new tile location
                    _ghost.Position = _ghost.Move(_ghost.Position, nextTilePosition, elapsedSeconds);
                    _ghost.AnimatedSprite.Update(ScalableGameTime.GameTime);
                }
            }
        }
    }
}