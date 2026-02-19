using GAlgoT2530.AI;
using GAlgoT2530.Engine;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace PacmanGame{
    public class TripTileManager : GameObject
    {
        private GameMap _gameMap;
        private TiledMapTileLayer _foodLayer;
        public LinkedList<Tile> _tripTiles = new LinkedList<Tile>();
        public Dictionary<Tile, bool> blockedTiles = new Dictionary<Tile, bool>(); 
        private Pacman _pacman;
        private VargasHCFSM _vargasHCFSM;
        public bool alert;
        private Tile _alertedTile;

        public TripTileManager() : base("TripTileManager")
        {
        }

        public override void Initialize()
        {
            //_gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");
            //_foodLayer = _gameMap.TiledMap.GetLayer<TiledMapTileLayer>("Food");
            _pacman = (Pacman)GameObjectCollection.FindByName("Pacman");
            // _vargasHCFSM = (VargasHCFSM)(GameObjectCollection.FindByName("Ghost") as Ghost)?.FSM;
            // if(_vargasHCFSM == null)
            // {
            //     Debug.WriteLine("VargasHCFSM is null in TripTileManager");
            // }
            // Debug.WriteLine("VargasHCFSM is not null in Trip TileManager");
            // dummy trip tiles for testing
            //_tripTiles.AddLast(new Tile(22, 15));
            alert = false;
        }

        public override void Update()
        {
            foreach(Tile tile in _tripTiles)
            {
                if(tile.Equals(_pacman._currTile))
                {
                    Debug.WriteLine("Pacman reached a trip tile at " + tile.Col + ", " + tile.Row);
                    //_vargasHCFSM.CurrentState = VargasHCFSM.State.Alert;
                    alert = true;
                    _alertedTile = tile;
                    break;
                }
            }
        }

        public Tile GetAlertedTile()
        {
            return _alertedTile;
        }
        // Work around to make sure that the game map is not null when assigned. The function is called by VargasHCFSM in its initialization.
        public void setMap()
        {
            if(_tripTiles.Count > 0)
            {
                return;
            }
            _gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");
            _foodLayer = _gameMap.TiledMap.GetLayer<TiledMapTileLayer>("Food");
            TiledMapObjectLayer tiledMapObjectLayer = _gameMap.TiledMap.GetLayer<TiledMapObjectLayer>("TripTiles");
            foreach (var obj in tiledMapObjectLayer.Objects)
            {
                Tile tripTile = Tile.ToTile(new Vector2(obj.Position.X, obj.Position.Y), _gameMap.TiledMap.TileWidth, _gameMap.TiledMap.TileHeight);
                _tripTiles.AddLast(tripTile);
                _foodLayer.SetTile((ushort)tripTile.Col, (ushort)tripTile.Row, 4);
                blockedTiles.Add(tripTile, false);
            }
        }
    }
}