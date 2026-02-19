using GAlgoT2530.Engine;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.Tiled;
using Microsoft.Xna.Framework;

namespace PacmanGame
{
    public class GameMap : GameObject
    {
        public TiledMap TiledMap { get; private set; }
        public TiledMapRenderer TiledMapRenderer { get; private set; }
		
	/********************************************************************************
        PROBLEM 7 : Define Tile Graph.

        HOWTOSOLVE : 1. Just uncomment the given code below.

        public TileGraph TileGraph { get; private set; }
    ********************************************************************************/
        public TileGraph TileGraph { get; private set; }
        // Defines the row and column of any navigable tile to construct the tile graph
        public ushort StartColumn;
        public ushort StartRow;
        public Tile goalTile;

        public GameMap(string name) : base(name)
        {
        }

		public override void LoadContent()
        {
            TiledMap = _game.Content.Load<TiledMap>("VargasSimpleMap");
        }

        public override void Initialize()
        {
            // Initialize tiled map renderer
            TiledMapRenderer = new TiledMapRenderer(_game.GraphicsDevice, TiledMap);

            // Get the Food layer from the tiled map
            TiledMapTileLayer foodLayer = TiledMap.GetLayer<TiledMapTileLayer>("Food");
            
        /********************************************************************************
            PROBLEM 7 : Construct tile graph from the food layer.

            HOWTOSOLVE : 1. Just uncomment the given code.

            TileGraph = new TileGraph();
            TileGraph.CreateFromTiledMapTileLayer(foodLayer, StartColumn, StartRow);
        ********************************************************************************/
            // Luqman Edits:
            // Setting home tile dynamically/automatically
            TiledMapObjectLayer wayPoints = TiledMap.GetLayer<TiledMapObjectLayer>("WayPoints");

            foreach (var obj in wayPoints.Objects)
            {
                if(obj.Name == "Home")
                {
                    Tile startTile = Tile.ToTile(new Vector2(obj.Position.X, obj.Position.Y), TiledMap.TileWidth, TiledMap.TileHeight);
                    StartColumn = (ushort)startTile.Col;
                    StartRow = (ushort)startTile.Row;
                    continue;
                }else if(obj.Name == "Goal")
                {
                    goalTile = Tile.ToTile(new Vector2(obj.Position.X, obj.Position.Y), TiledMap.TileWidth, TiledMap.TileHeight);
                    continue;
                }
            }

            TileGraph = new TileGraph();
            TileGraph.CreateFromTiledMapTileLayer(foodLayer, StartColumn, StartRow);
            
        }
        // Luqman: Edited so that the map is updated.
        public override void Update()
        {
            TiledMapRenderer = new TiledMapRenderer(_game.GraphicsDevice, TiledMap);
            TiledMapRenderer.Update(ScalableGameTime.GameTime);
        }

        public override void Draw()
        {
            TiledMapRenderer.Draw();
        }
    }
}
