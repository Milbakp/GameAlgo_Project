// using var game = new PacmanGame.Game1();
// Have to manually set window size here
using var game = new GAlgoT2530.Engine.GameEngine("Pacman Game", 1200, 720);
game.AddScene("PacmanScene", new PacmanGame.PacmanScene());
game.Run();
