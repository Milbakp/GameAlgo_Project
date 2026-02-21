using GAlgoT2530.Engine;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PacmanGame
{
    public class ScoreUI : GameObject
    {
        // UI Variables
        private SpriteFont _scoreFont;

        public int winScore;
        public int loseScore;
        public override void Initialize()
        {
            _scoreFont = _game.Content.Load<SpriteFont>("ScoreFont");
            winScore = 0;
            loseScore = 0;

        }

        public override void Draw()
        {
            _game.SpriteBatch.Begin();
            
            string scoreText = $"Wins: {winScore}  Losses: {loseScore}";
            Vector2 textSize = _scoreFont.MeasureString(scoreText);

            // Viewport.Width / 2 is the middle of the window
            // textSize.X / 2 is the middle of the words
            float centerX = (_game.GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2);
            float centerY = 20; // Keep it near the top
            Vector2 position = new Vector2(centerX, centerY);

            _game.SpriteBatch.DrawString(_scoreFont, scoreText, position, Color.White);

            _game.SpriteBatch.End();
        }
    }
}