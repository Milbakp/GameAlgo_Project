// Main script for Assignment 2
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
    public class CollectPowerPelletHCFSM : HCFSM
    {
        // What each state does:
        // STOP: Ghost looks through all the paths to the power pellet's that are left and pick the shortest one.
        // MOVING: Ghost moving to pick up the current power pellet.
        // RETURNING: Ghost returns to the home tile.
        // GOAL: After collecting the last power pellet goes to the goal tile.
        // COMPLTED: After reaching the goal tile, do nothing.
        public enum NavigationState { STOP, MOVING, RETURNING, GOAL, COMPLETED };

        // Navigation current state
        private NavigationState _currentState = NavigationState.STOP;

        // Navigation
        private Tile _srcTile;
        private Tile _destTile;
        private Tile _goalTile;
        private Ghost _ghost;
        private TiledMap _tiledMap;
        private TileGraph _tileGraph;
        private List<LinkedList<Tile>> _powerPelletTilesOrder = new List<LinkedList<Tile>>();
        private LinkedList<Tile> _shortestPath;

        public CollectPowerPelletHCFSM(Ghost ghost, NavigationState currentState)
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

            // Getting the WayPoints layer from the tiled map
            TiledMapObjectLayer wayPoints = _tiledMap.GetLayer<TiledMapObjectLayer>("WayPoints");
            List<Tile> powerPelletTiles = new List<Tile>();

            // filling _powerPelletTilesOrder with all paths to each power pellet.
            foreach (var obj in wayPoints.Objects)
            {
                if(obj.Name == "Home")
                {
                    continue;
                }else if(obj.Name == "Goal")
                {
                    _goalTile = Tile.ToTile(new Vector2(obj.Position.X, obj.Position.Y), _tiledMap.TileWidth, _tiledMap.TileHeight);
                    continue;
                }
                Tile tmpTile = Tile.ToTile(new Vector2(obj.Position.X, obj.Position.Y), _tiledMap.TileWidth, _tiledMap.TileHeight);
                LinkedList<Tile> tmpPath = AStar.Compute(_tileGraph, _srcTile, tmpTile, AStarHeuristic.EuclideanSquared);
                tmpPath.RemoveFirst(); // Remove the source tile from the path
                _powerPelletTilesOrder.Add(tmpPath);
                Debug.WriteLine($"Power Pellet Tile at (Col = {tmpTile.Col}, Row = {tmpTile.Row}). Takes {tmpPath.Count} steps to reach.");
            }
            // Initialize Destination Tile
            _destTile = Tile.ToTile(new Vector2(0, 0), _tiledMap.TileWidth, _tiledMap.TileHeight);

        }
        public override void Update()
        {
            int tileWidth = _tiledMap.TileWidth;
            int tileHeight = _tiledMap.TileHeight;

            float elapsedSeconds = ScalableGameTime.DeltaTime;
            // MOVING, RETURNING and GOAL all involve movement to the _destTile hence this if statement.
            if (_currentState == NavigationState.MOVING || _currentState == NavigationState.RETURNING
                || _currentState == NavigationState.GOAL){
                // Arriving to the _destTile
                if (_shortestPath.Count == 0 ||
                    _ghost.Position.Equals(Tile.ToPosition(_destTile, tileWidth, tileHeight))
                   )
                {
                    // Update source tile to destination tile
                    _srcTile = _destTile;
                    _destTile = null;

                    if(_currentState == NavigationState.MOVING)
                    {
                        GameMap gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");   
                        if (_powerPelletTilesOrder.Count == 0)
                        {
                            _destTile = _goalTile;
                            _shortestPath = AStar.Compute(_tileGraph, _srcTile, _destTile, AStarHeuristic.EuclideanSquared);
                            _shortestPath.RemoveFirst(); // Remove the source tile from the path
                            _currentState = NavigationState.GOAL;
                        }
                        else
                        {
                            _destTile = new Tile(gameMap.StartColumn, gameMap.StartRow);
                            _shortestPath = AStar.Compute(_tileGraph, _srcTile, _destTile, AStarHeuristic.EuclideanSquared);
                            _shortestPath.RemoveFirst(); // Remove the source tile from the path
                            _currentState = NavigationState.RETURNING; 
                        }
                        // Changing the tile of the collected power pellet to empty tile
                        TiledMapTileLayer foodLayer = gameMap.TiledMap.GetLayer<TiledMapTileLayer>("Food");
                        foodLayer.SetTile((ushort)_srcTile.Col, (ushort)_srcTile.Row, 5);
                        
                    }
                    else if (_currentState == NavigationState.RETURNING)
                    {
                        _currentState = NavigationState.STOP;
                    }
                    else if (_currentState == NavigationState.GOAL)
                    {
                        _currentState = NavigationState.COMPLETED;
                    }
                }
                else
                {
                    Tile nextTile = _shortestPath.First.Value; // throw exception if path is empty

                    Vector2 nextTilePosition = Tile.ToPosition(nextTile, tileWidth, tileHeight);

                    if (_ghost.Position.Equals(nextTilePosition))
                    {
                        Debug.WriteLine($"Reached the next tile (Col = {nextTile.Col}, Row = {nextTile.Row}).");
                        Debug.WriteLine($"Removing this tile from the path and getting the new next tile from path.");
                        
                        // Get the position of the new next tile from the path
                        _shortestPath.RemoveFirst();
                        Tile newNextTile = _shortestPath.First.Value;
                        nextTilePosition = Tile.ToPosition(newNextTile, tileWidth, tileHeight);

                        // Update the animation
                        _ghost.UpdateAnimatedSprite(nextTile, newNextTile);
                    }

                    // Move the ghost to the new tile location
                    _ghost.Position = _ghost.Move(_ghost.Position, nextTilePosition, elapsedSeconds);
                    _ghost.AnimatedSprite.Update(ScalableGameTime.GameTime);
                }
            }
            else if (_currentState == NavigationState.STOP)
            {
                int currentShortest = 1000000;
                foreach (var path in _powerPelletTilesOrder)
                {
                    if (path.Count < currentShortest && path.Count > 0)
                    {
                        currentShortest = path.Count;
                        _shortestPath = path;
                    }
                }
                _powerPelletTilesOrder.Remove(_shortestPath);
                _destTile = _shortestPath.Last.Value;
                _currentState = NavigationState.MOVING;
            }
            else if (_currentState == NavigationState.COMPLETED)
            {
                // Do nothing, completed collecting all power pellets and reached goal
            }
        }
    }
}