// Handles the score UI and displaying win/lose text
using GAlgoT2530.Engine;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PacmanGame
{
    public class ScoreUI : GameObject
    {
        public enum GameState { Win, Lose, Playing };
        public GameState CurrentGameState;
        private SpriteFont _scoreFont, _winLoseFont;
        public int winScore;
        public int loseScore;
        
        public override void Initialize()
        {
            _scoreFont = _game.Content.Load<SpriteFont>("ScoreFont");
            _winLoseFont = _game.Content.Load<SpriteFont>("WinLoseFont");
            winScore = 0;
            loseScore = 0;
            CurrentGameState = GameState.Playing;
        }

        public override void Draw()
        {
            _game.SpriteBatch.Begin();
            
            string scoreText = $"Wins: {winScore}  Losses: {loseScore}";
            Vector2 textSize = _scoreFont.MeasureString(scoreText);

            float centerX = (_game.GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2);
            float centerY = 20; // Keep it near the top
            Vector2 position = new Vector2(centerX, centerY);

            _game.SpriteBatch.DrawString(_scoreFont, scoreText, position, Color.White);

            _game.SpriteBatch.End();

            _game.SpriteBatch.Begin();
                if(CurrentGameState == GameState.Win)
                {
                    string winText = "You Win!";
                    Vector2 winTextSize = _winLoseFont.MeasureString(winText);
                    Vector2 winPosition = new Vector2((_game.GraphicsDevice.Viewport.Width / 2) - (winTextSize.X / 2), (_game.GraphicsDevice.Viewport.Height / 2) - (winTextSize.Y / 2));
                    _game.SpriteBatch.DrawString(_winLoseFont, winText, winPosition, Color.Green);
                }
                else if(CurrentGameState == GameState.Lose)
                {
                    string loseText = "You Lose!";
                    Vector2 loseTextSize = _winLoseFont.MeasureString(loseText);
                    Vector2 losePosition = new Vector2((_game.GraphicsDevice.Viewport.Width / 2) - (loseTextSize.X / 2), (_game.GraphicsDevice.Viewport.Height / 2) - (loseTextSize.Y / 2));
                    _game.SpriteBatch.DrawString(_winLoseFont, loseText, losePosition, Color.Red);
                }
            _game.SpriteBatch.End();

        }
    }
}