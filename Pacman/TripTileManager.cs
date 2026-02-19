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
        private Pacman _pacman;
        private VargasHCFSM _vargasHCFSM;
        private LinkedList<Tile> _tripTiles = new LinkedList<Tile>();
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
            _tripTiles.AddLast(new Tile(22, 15));
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
    }
}