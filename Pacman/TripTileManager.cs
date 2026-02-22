// Manages the trip tiles, including their states and checking if pacman is on an active trip tile.
// Activates half of the trip tiles according to the heat map values.
using GAlgoT2530.AI;
using GAlgoT2530.Engine;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Microsoft.Xna.Framework.Audio;

namespace PacmanGame{
    public class TripTileManager : GameObject
    {
        private GameMap _gameMap;
        private TiledMapTileLayer _foodLayer;
        public LinkedList<Tile> _tripTiles = new LinkedList<Tile>();
        public Dictionary<Tile, bool> blockedTiles = new Dictionary<Tile, bool>(); 
        public Dictionary<Tile, bool> activeTiles = new Dictionary<Tile, bool>(); 
        private Pacman _pacman;
        public bool alert;
        private Tile _alertedTile;
        private SoundEffect TripTileSound;

        public TripTileManager() : base("TripTileManager")
        {
        }

        public override void Initialize()
        {
            _pacman = (Pacman)GameObjectCollection.FindByName("Pacman");
            alert = false;
            TripTileSound = _game.Content.Load<SoundEffect>("Alarm");
        }
        // Checks if pacman is on an active trip tile, if yes, sets alert to true and stores the alerted tile.
        public override void Update()
        {
            foreach(Tile tile in _tripTiles)
            {
                if(tile.Equals(_pacman._currTile) && activeTiles[tile])
                {
                    TripTileSound.Play();
                    Debug.WriteLine("Pacman reached a trip tile at " + tile.Col + ", " + tile.Row);
                    alert = true;
                    _alertedTile = tile;
                    break;
                }
            }
        }
        // Getter for the alerted tile, used by VargasHCFSM to know travel to when in alert state.
        public Tile GetAlertedTile()
        {
            return _alertedTile;
        }
        // Work around to make sure that the game map is not null when assigned. The function is called by VargasHCFSM in its initialization.
        public void setMap()
        {
            // Only assign the map and food layer on the first call.
            if(!(_tripTiles.Count > 0))
            {
                _gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");
                _foodLayer = _gameMap.TiledMap.GetLayer<TiledMapTileLayer>("Food");
            }

            TiledMapObjectLayer tiledMapObjectLayer = _gameMap.TiledMap.GetLayer<TiledMapObjectLayer>("TripTiles");
            foreach (var obj in tiledMapObjectLayer.Objects)
            {
                // Adding the trip tiles on the first call.
                Tile tripTile = Tile.ToTile(new Vector2(obj.Position.X, obj.Position.Y), _gameMap.TiledMap.TileWidth, _gameMap.TiledMap.TileHeight);
                if (!_tripTiles.Contains(tripTile))
                {
                    _tripTiles.AddLast(tripTile);
                    blockedTiles.Add(tripTile, false);
                    activeTiles.Add(tripTile, false);
                    continue;
                }
                // Setting the tiles to default states.
                _foodLayer.SetTile((ushort)tripTile.Col, (ushort)tripTile.Row, 5);
                blockedTiles[tripTile] = false;
                activeTiles[tripTile] = false;
            }
            // Random initial trip tiles
            if(_pacman == null)
            {
                Random rng = new Random();
                var randomIndices = Enumerable.Range(0, _tripTiles.Count)
                                            .OrderBy(x => rng.Next()) 
                                            .Take(_tripTiles.Count / 2);
                
                foreach (int index in randomIndices)
                {
                    Tile tile = _tripTiles.ElementAt(index);
                    activeTiles[tile] = true;
                    _foodLayer.SetTile((ushort)tile.Col, (ushort)tile.Row, 4);
                }
            }
            else
            {
                // Getting all trip tiles and their corresponding heat map values, 
                // then activating the top half of them with the highest heat map values.
                Dictionary<Tile, float> tripTilesToActivate = new Dictionary<Tile, float>();
                foreach(Tile tile in _tripTiles)
                {
                    tripTilesToActivate[tile] = _pacman.HeatMap[(tile.Col, tile.Row)];
                }
                int limit = (int)Math.Ceiling(_tripTiles.Count / 2.0);
                Dictionary<Tile, float> topHalf = tripTilesToActivate.OrderByDescending(x => x.Value)
                    .Take(limit)
                    .ToDictionary(x => x.Key, x => x.Value);
                foreach (Tile tile in topHalf.Keys)
                {
                    activeTiles[tile] = true;
                    _foodLayer.SetTile((ushort)tile.Col, (ushort)tile.Row, 4);
                }
            }
        }
    }
}