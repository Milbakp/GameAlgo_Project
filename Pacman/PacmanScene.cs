using GAlgoT2530.Engine;


namespace PacmanGame
{
    public class PacmanScene : GameScene
    {
        private GameMap _gameMap;
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
            Ghost ghost = new Ghost();

            Pacman pacman = new Pacman();
            pacman.Speed = 100.0f;
            pacman.StartRow = 28;
            pacman.StartColumn = 0;
            pacman.NavigableTileLayerName = "Food";
        }
        // Updating the map, replaces the power pellet tile with tthe empty tile.
        public override void Update()
        {
            _gameMap.Update();
        }
    }
}
