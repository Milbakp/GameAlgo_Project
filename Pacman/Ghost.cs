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
    public class Ghost : AnimationGameObject // GameObject
    {
        // FSM for navigation
        public HCFSM FSM;
        public float MaxSpeed;
        // public Texture2D Texture;

        // Visual appearance
        private Rectangle _ghostRect;

        public Ghost() : base("Vargas","vargasAnimations.sf")
        {
        }

        public override void Initialize()
        {
            //_gameEngine = new GameEngine("Pacman Game", 696, 432);

            // For normal ghost behaviors from lab 11
            // GameMap gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");
            // Tile startTile = new Tile(gameMap.StartColumn, gameMap.StartRow);
            // Position = Tile.ToPosition(startTile, gameMap.TiledMap.TileWidth, gameMap.TiledMap.TileHeight);
            // Pacman pacman = (Pacman)GameObjectCollection.FindByName("Pacman");
            // FSM = new GhostHCFSM(_game, this, gameMap.TiledMap, gameMap.TileGraph, pacman);

            //FSM = new NavigationHCFSM(this, NavigationHCFSM.NavigationState.STOP);
            // Assingment 2
            //FSM = new CollectPowerPelletHCFSM(this, CollectPowerPelletHCFSM.NavigationState.STOP);

            // Project
            GameMap gameMap = (GameMap)GameObjectCollection.FindByName("GameMap");
            Tile startTile = new Tile(gameMap.StartColumn, gameMap.StartRow);
            Position = Tile.ToPosition(startTile, gameMap.TiledMap.TileWidth, gameMap.TiledMap.TileHeight);
            Pacman pacman = (Pacman)GameObjectCollection.FindByName("Pacman");
            FSM = new VargasHCFSM(_game, this, gameMap.TiledMap, gameMap.TileGraph, pacman);
            FSM.Initialize();
        }

        public override void Update()
        {
            FSM.Update();
        }

        public override void Draw()
        {
            // Draw the ghost at his position, extracting only the ghost image from the texture
            _game.SpriteBatch.Begin();

            // Commented out as Texture is not used anymore
            // _game.SpriteBatch.Draw(Texture, Position, _ghostRect, Color.White, Orientation, Origin, Scale, SpriteEffects.None, 0f);

            /********************************************************************************
                PROBLEM 3(D): Draw the animation using extended SpriteBatch.Draw() for AnimatedSprite at a given position, orientation, and scale.

                HOWTOSOLVE : 1. Copy the code below.
                             2. Paste it below this block comment.
                             3. Fill in the blanks.

                
                // _game.SpriteBatch.Draw(________, Position, ________, Scale);

            ********************************************************************************/
            _game.SpriteBatch.Draw(AnimatedSprite, Position, Orientation, Scale);
            _game.SpriteBatch.End();
        }
        // Given source (src) and destination (dest) locations, and elapsed time, 
        //     try to move from source to destination at the given speed within elapsed time.
        // If cannot reach dest within the elapsed time, return the location where it will reach.
        // If can reach or overshoot the dest, the return dest.
        public Vector2 Move(Vector2 src, Vector2 dest, float elapsedSeconds)
        {
            Vector2 dP = dest - src;
            float distance = dP.Length();
            float step = MaxSpeed * elapsedSeconds;

            if (step < distance)
            {
                dP.Normalize();
                return src + (dP * step);
            }
            else
            {
                return dest;
            }
        }

        // Select the ghost current animation based on:
        // (a) Which tile the ghost is standing on (ghostTile)
        // (b) Which tile the ghost is heading next (nextTile)
        //
        // Pre-conditions:
        //    The animation name is suffixed by:
        //      "NorthWest", "Up", "NorthEast", "Left", "Centre", "Right", "SouthWest", "Down", "SouthEast"
        //
        // Example:
        //    If nextTile is on the right of ghostTile, the animation to play is "ghostRedRight".
        public void UpdateAnimatedSprite(Tile ghostTile, Tile nextTile)
        {
            // string[] directions = {"NorthWest", "Up"    , "NorthEast",
            //                        "Left"     , "Centre", "Right"    ,
            //                        "SouthWest", "Down"  , "SouthEast"};
            // Tmp solution for weird bug that heppens when switching states
            string[] directions = {"Up", "Up"    , "Right",
                                   "Left"     , "Down", "Right"    ,
                                   "Left", "Down"  , "Down"};


            if (ghostTile == null || nextTile == null)
            {
                throw new ArgumentNullException("UpdateAnimatedSprite(): ghostTile or nextTile is null.");
            }
            else
            {
            /********************************************************************************
                PROBLEM 3(B): Compute the index value that refer to the correct animation
                              suffix in the 'directions' array based on ghost tile and next
                              tile.


                HOWTOSOLVE : 1. Write your own code.

                // You may write more lines of code before the code below to compute the index.
                int index = ?;

            ********************************************************************************/
                Tile difference = new Tile(nextTile.Col - ghostTile.Col, nextTile.Row - ghostTile.Row);
                int index = (difference.Col + 1) + 3 * (difference.Row + 1);

                //Console.WriteLine($"Direction: {directions[index]}");
                //Debug.WriteLine($"Col: {nextTile.Col}, Row: {nextTile.Row}");
                string animationName = $"ghostRed{directions[index]}";

                if (AnimatedSprite.CurrentAnimation != animationName)
                {
                    AnimatedSprite.SetAnimation(animationName);
                    AnimatedSprite.TextureRegion = SpriteSheet.TextureAtlas[AnimatedSprite.Controller.CurrentFrame];
                }
            }
        }
    }
}
