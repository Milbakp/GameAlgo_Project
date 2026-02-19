// Based from GhostHCGSM.cs
using GAlgoT2530.AI;
using GAlgoT2530.Engine;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace PacmanGame
{
    public class VargasHCFSM : HCFSM
    {
        public enum State { Roam, Chase, HeatMap, Alert, Scanning};

        public State CurrentState;
        private GameEngine _game;
        private Ghost _ghost;
        private TiledMap _tiledMap;
        private TileGraph _tileGraph;

        private Tile _srcTile;
        private Tile _destTile;
        private LinkedList<Tile> _path;

        // For Roam State only
        private Vector2 _destTilePosition;

        // For Chase State only
        private Pacman _pacman;
        private Tile _nextTile;
        private Vector2 _nextTilePosition;
		
		// For transitions between Roam and Chase states
        public float RoamMaxSeconds;
        public float ChaseMaxSeconds;
        private float _roamRemainingSeconds;
        private float _chaseRemainingSeconds;

        // New variables
        // public enum Direction {Up,Down,Left,Right,};
        // public Direction direction;
        public string direction;
        public TripTileManager _tripTileManager;
        private Tile _goalTile;

        // Lab 11 (Problem 3): New Constructor
        public VargasHCFSM(GameEngine game, Ghost ghost, TiledMap map, TileGraph graph, Pacman pacman)
        {
            _path = new LinkedList<Tile>();

            _game = game;
            _ghost = ghost;
            _tiledMap = map;
            _tileGraph = graph;
            _pacman = pacman;
            _tripTileManager = (TripTileManager)GameObjectCollection.FindByName("TripTileManager");
            // if (_tripTileManager == null)
            // {
            //     Debug.WriteLine("Error: TripTileManager not found in GameObjectCollection!");
            // }
            // else
            // {
            //     Debug.WriteLine("TripTileManager found in GameObjectCollection!");
            // }
        }

        // Lab 11 (Problem 3): New Initialize()
        public override void Initialize()
        {
            CurrentState = State.Roam;

            // Comment out this line to test the Chase state below.
            // Roam_Initialize();

			// Comment out for Lab 11 (Problem 4)
            // Chase_Initialize();

            // Lab 11 (Problem 4) only
            RoamMaxSeconds = 3f;
            ChaseMaxSeconds = 5f;
            _roamRemainingSeconds = 0f;
            _chaseRemainingSeconds = 0f;

            TiledMapObjectLayer wayPoints = _tiledMap.GetLayer<TiledMapObjectLayer>("WayPoints");

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
            }
            _ghost.Position = Tile.ToPosition(_goalTile, _tiledMap.TileWidth, _tiledMap.TileHeight);
			
            Roam_Initialize();
        }

        // Lab 11 (Problem 3): New Update()
        public override void Update()
        {
            // Comment out this line to test Chase action
            // Roam_Action();

            // Lab 11 (Problem 3)
            // Chase_Action();

		if (CurrentState == State.Roam)
            {
                // if (TransitionTriggered_RoamToChase())
                // {
                //     Chase_Initialize();
                //     CurrentState = State.Chase;
                // }
                // else
                // {
                //     Roam_Action();
                // }
                Roam_Action();
            }
            else if (CurrentState == State.Chase)
            {
                // if (TransitionTriggered_ChaseToRoam())
                // {
                //     Roam_Initialize();
                //     CurrentState = State.Roam;
                // }
                // else
                // {
                //     Chase_Action();
                // }
                Chase_Action();
            }
            else if(CurrentState == State.HeatMap)
            {
                HeatMap_Action();
            }
            else if(CurrentState == State.Alert)
            {
                Alert_Action();
            }

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.H))
            {
                CurrentState = State.HeatMap;
                HeatMap_Initialize();
            }
            if(_tripTileManager.alert == true)
            {
                _tripTileManager.alert = false;
                CurrentState = State.Alert;
                Alert_Initialize();
            }
        }

        /// ROAM STATE /// <summary>
        #region ROAM STATE METHODS
        /// </summary>
        private void Roam_Initialize()
        {
            // Set the ghost Speed
            _ghost.MaxSpeed = 100.0f;

            // Initialize source tile from owner's current position
            _srcTile = Tile.ToTile(_ghost.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);
        }

        private void Roam_Action()
        {
			// Lab11 (Problem 4): Update roam countdown timer
            _roamRemainingSeconds -= ScalableGameTime.DeltaTime;
		
            if (Roam_IsPathEmpty())
            {
				// Update source tile
			    _srcTile = Tile.ToTile(_ghost.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);

			    // Get new random path and remove the source tile from the path
			    _path = Roam_GenerateRandomPath(_tileGraph, _srcTile);
			    _path.RemoveFirst();

			    // Update destination tile and its position
			    _destTile = _path.Last.Value;
			    _destTilePosition = Tile.ToPosition(_destTile, _tiledMap.TileWidth, _tiledMap.TileHeight);

			    // Change animation
			    Tile nextTile = _path.First.Value;
			    _ghost.UpdateAnimatedSprite(_srcTile, nextTile);
            }

            Roam_Moving();
        }

        private bool Roam_IsPathEmpty()
        {
            return _path == null || _path.Count == 0;
        }

        private LinkedList<Tile> Roam_GenerateRandomPath(TileGraph graph, Tile srcTile)
        {
			// Randomly select a navigable tile as destination
            Tile randomTile = new Tile(-1, -1);
            while (!_tileGraph.Nodes.Contains(randomTile) ||
                   randomTile.Equals(srcTile)
                  )
            {
                randomTile.Col = _game.Random.Next(0, _tiledMap.Width);
                randomTile.Row = _game.Random.Next(0, _tiledMap.Height);
            }

            // Compute an A* path
            return AStar.Compute(_tileGraph, srcTile, randomTile, AStarHeuristic.EuclideanSquared);
        }

        private void Roam_Moving()
        {
            float elapsedSeconds = ScalableGameTime.DeltaTime;

            int tileWidth = _tiledMap.TileWidth;
            int tileHeight = _tiledMap.TileHeight;
			
			Tile headTile = _path.First.Value;
            Vector2 headTilePosition = Tile.ToPosition(headTile, tileWidth, tileHeight);

            if (_ghost.Position.Equals(headTilePosition))
            {
                Debug.WriteLine($"Reach head tile (Col = {headTile.Col}, Row = {headTile.Row}).");

                // Remove the head tile from path
                Debug.WriteLine($"Removing head tile. Get next node from path.");
                _path.RemoveFirst();

                // Update animation
                if (!Roam_IsPathEmpty())
                {
                    Tile nextTile = _path.First.Value;
                    _ghost.UpdateAnimatedSprite(headTile, nextTile);
                    facingDirection(headTile, nextTile);
                }
            }
		    //facingDirection(_srcTile, _path.First.Value);
            // Move the ghost to the new tile location
            _ghost.Position = _ghost.Move(_ghost.Position, headTilePosition, elapsedSeconds);
            _ghost.AnimatedSprite.Update(ScalableGameTime.GameTime);

        }
        #endregion

        //// CHASE STATE ////
        #region CHASE STATE METHODS
        private void Chase_Initialize()
        {
            _ghost.MaxSpeed = 120.0f;

            // Initialize the next tile position
            _nextTilePosition = _ghost.Position;
            _nextTile = Tile.ToTile(_nextTilePosition, _tiledMap.TileWidth, _tiledMap.TileHeight);
        }

        private void Chase_Action()
        {
            float elapsedSeconds = ScalableGameTime.DeltaTime;

            // Lab11 (Problem 4): Update chase countdown timer

            _chaseRemainingSeconds -= ScalableGameTime.DeltaTime;
            // Reach the next tile
            if (_ghost.Position.Equals(_nextTilePosition))
            {
                if (Chase_ShouldRecalculatePathTowardsPacman())
                {
                    Chase_RecalculatePathTowardsPacman();
                }
                else
                {
                    // Get the next tile and update animation
                    _srcTile = _nextTile;
                    _path.RemoveFirst();
                    if (!_srcTile.Equals(_destTile))
                    {
                        _nextTile = _path.First.Value;
                        _nextTilePosition = Tile.ToPosition(_nextTile, _tiledMap.TileWidth, _tiledMap.TileHeight);
                        _ghost.UpdateAnimatedSprite(_srcTile, _nextTile);
                        facingDirection(_srcTile, _nextTile);
                    }
                }
            }
            // Move the ghost to the new tile location
            _ghost.Position = _ghost.Move(_ghost.Position, _nextTilePosition, elapsedSeconds);
            _ghost.AnimatedSprite.Update(ScalableGameTime.GameTime);
            
            
        }

        private bool Chase_ShouldRecalculatePathTowardsPacman()
        {
            if (_path != null)
            {
                // Get the pacman's tile as destination tile
                _destTile = Tile.ToTile(_pacman.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);

                // Clean up the path from last node to first node
                //   until the last node either contains the destination tile
                LinkedListNode<Tile> destTileNode = _path.Find(_destTile);
                while (_path.Last != destTileNode)
                {
                    _path.RemoveLast();
                }

                return destTileNode == null;
            }
            else
            {
                return true;
            }
        }

        private void Chase_RecalculatePathTowardsPacman()
        {
            // Precondition:
            // 1. Owner must be position exactly on the source tile
            // Postconditions:
            // 1. Updates srcTile, destTile, path, nextTile, nextTilePosition

            _srcTile = Tile.ToTile(_ghost.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);
            _destTile = Tile.ToTile(_pacman.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);

                // 1. Compute an A* path
                _path.Clear();
                _path = AStar.Compute(_tileGraph, _srcTile, _destTile, AStarHeuristic.EuclideanSquared);

                // 2. Remove the source tile from the path 
                _path.RemoveFirst();

                // 3. Update ghost animation
                if (!_srcTile.Equals(_destTile))
                {
                    // Calculate next tile for the purpose of playing the correct animation.
                    _nextTile = _path.First.Value;
                    _nextTilePosition = Tile.ToPosition(_nextTile, _tiledMap.TileWidth, _tiledMap.TileHeight);

                    // Ensure animated starts at the correct direction.
                    _ghost.UpdateAnimatedSprite(_srcTile, _nextTile);
                }
            
        }
        #endregion

        //// TRANSITIONS ////
        //// Lab11 (Problem 4)
		private bool TransitionTriggered_RoamToChase()
        {
        // Roaming has no seconds remaining
            if (_roamRemainingSeconds <= 0)
            {
                // Reset roaming seconds
                _roamRemainingSeconds = 3;
                return true;
            }

            return false;
			
        }

        private bool TransitionTriggered_ChaseToRoam()
        {
            if (_chaseRemainingSeconds <= 0)
            {
                _chaseRemainingSeconds = 5;
                return true;
            }

            return false;
			
        }
        //// HeatMap STATE ////
        #region HeatMap State Methods
        private void HeatMap_Initialize()
        {
            _ghost.MaxSpeed = 120.0f;

            // Initialize the next tile position
            _nextTilePosition = _ghost.Position;
            _nextTile = Tile.ToTile(_nextTilePosition, _tiledMap.TileWidth, _tiledMap.TileHeight);
            _path.Clear();
            KeyValuePair<(int x, int y), float> maxEntry = default;
            bool first = true;

            foreach (var entry in _pacman.HeatMap)
            {
                if (first || entry.Value > maxEntry.Value)
                {
                    maxEntry = entry;
                    first = false;
                }
            }
            _srcTile = Tile.ToTile(_ghost.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);
            _destTile = new Tile(maxEntry.Key.x, maxEntry.Key.y);
            _path = AStar.Compute(_tileGraph, _srcTile, _destTile, AStarHeuristic.EuclideanSquared);
            _path.RemoveFirst();

            if (!_srcTile.Equals(_destTile))
                {
                    // Calculate next tile for the purpose of playing the correct animation.
                    _nextTile = _path.First.Value;
                    _nextTilePosition = Tile.ToPosition(_nextTile, _tiledMap.TileWidth, _tiledMap.TileHeight);

                    // Ensure animated starts at the correct direction.
                    _ghost.UpdateAnimatedSprite(_srcTile, _nextTile);
                }
        }
        private void HeatMap_Action()
        {
            float elapsedSeconds = ScalableGameTime.DeltaTime;
            // Reach the next tile
            if (_ghost.Position.Equals(_nextTilePosition) && _path.Count > 0)
            {
                    // Get the next tile and update animation
                    _srcTile = _nextTile;
                    _path.RemoveFirst();
                    if (!_srcTile.Equals(_destTile))
                    {
                        _nextTile = _path.First.Value;
                        _nextTilePosition = Tile.ToPosition(_nextTile, _tiledMap.TileWidth, _tiledMap.TileHeight);
                        _ghost.UpdateAnimatedSprite(_srcTile, _nextTile);
                        facingDirection(_srcTile, _nextTile);
                    }
            }
            if (_srcTile.Equals(_destTile))
            {
                Roam_Initialize();
                CurrentState = State.Roam;
            }
            // Move the ghost to the new tile location
            _ghost.Position = _ghost.Move(_ghost.Position, _nextTilePosition, elapsedSeconds);
            _ghost.AnimatedSprite.Update(ScalableGameTime.GameTime);
        }
        #endregion

        //// Alert STATE ////
        #region Alert State Methods
        private void Alert_Initialize()
        {
            _ghost.MaxSpeed = 150.0f;

            // Initialize the next tile position
            _nextTilePosition = _ghost.Position;
            _nextTile = Tile.ToTile(_nextTilePosition, _tiledMap.TileWidth, _tiledMap.TileHeight);
            _path.Clear();

            _srcTile = Tile.ToTile(_ghost.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);
            _destTile = _tripTileManager.GetAlertedTile();
            _path = AStar.Compute(_tileGraph, _srcTile, _destTile, AStarHeuristic.EuclideanSquared);
            _path.RemoveFirst();

            if (!_srcTile.Equals(_destTile))
                {
                    // Calculate next tile for the purpose of playing the correct animation.
                    _nextTile = _path.First.Value;
                    _nextTilePosition = Tile.ToPosition(_nextTile, _tiledMap.TileWidth, _tiledMap.TileHeight);

                    // Ensure animated starts at the correct direction.
                    _ghost.UpdateAnimatedSprite(_srcTile, _nextTile);
                }
        }

        private void Alert_Action()
        {
            float elapsedSeconds = ScalableGameTime.DeltaTime;
            // Reach the next tile
            if (_ghost.Position.Equals(_nextTilePosition) && _path.Count > 0)
            {
                    // Get the next tile and update animation
                    _srcTile = _nextTile;
                    _path.RemoveFirst();
                    if (!_srcTile.Equals(_destTile))
                    {
                        _nextTile = _path.First.Value;
                        _nextTilePosition = Tile.ToPosition(_nextTile, _tiledMap.TileWidth, _tiledMap.TileHeight);
                        _ghost.UpdateAnimatedSprite(_srcTile, _nextTile);
                        facingDirection(_srcTile, _nextTile);
                    }
            }
            if (_srcTile.Equals(_destTile))
            {
                Roam_Initialize();
                CurrentState = State.Roam;
            }
            // Move the ghost to the new tile location
            _ghost.Position = _ghost.Move(_ghost.Position, _nextTilePosition, elapsedSeconds);
            _ghost.AnimatedSprite.Update(ScalableGameTime.GameTime);
        }
        #endregion

        // Luqman: Code
        // Determine's Vargas's facing direction and scans all tiles in that direction until a wall is hit, 
        // to check if Pacman is in vision range.
        private void facingDirection(Tile ghostTile, Tile nextTile)
        {
            // string[] directions = {"NorthWest", "Up"    , "NorthEast",
            //                        "Left"     , "Centre", "Right"    ,
            //                        "SouthWest", "Down"  , "SouthEast"};

            Tile difference = new Tile(nextTile.Col - ghostTile.Col, nextTile.Row - ghostTile.Row);
            int index = (difference.Col + 1) + 3 * (difference.Row + 1);

            // direction = directions[index];
            // Debug.WriteLine($"Direction: {direction}");

            int[][] directionsNum = {[0,0],  [0,1], [0,0],
                                   [-1,0], [0,0], [1,0],
                                   [0,0], [0,-1], [0,0]};
            int[] directionNum = directionsNum[index];

            TiledMapTileLayer foodLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Food");
            TiledMapTile? tile;
            Tile pacmanTile = Tile.ToTile(_pacman.Position, _tiledMap.TileWidth, _tiledMap.TileHeight);
            while(true)
            {
                if (foodLayer.TryGetTile((ushort)nextTile.Col, (ushort)nextTile.Row, out tile)) 
                {
                    if (tile.Value.IsBlank) 
                    {  
                        // No tile at this position, stop looking further in this direction
                        //Debug.WriteLine($"No tile at ({nextTile.Col}, {nextTile.Row}). Stopping vision in this direction.");
                        break;
                    }
                    // There is a tile here
                    // var gid = tile.Value.GlobalIdentifier;
                    //Debug.WriteLine($"There is a tile at ({nextTile.Col}, {nextTile.Row}) with GID: {gid}");
                }
                if (nextTile.Equals(pacmanTile) && !(CurrentState == State.Chase))
                {
                    Debug.WriteLine("Pacman is in vision range.");
                    Chase_Initialize();
                    CurrentState = State.Chase;
                    break;
                }
                nextTile = new Tile(nextTile.Col + directionNum[0], nextTile.Row + directionNum[1]);
            }
            
        }
    }
}
