using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ISS_NBackCircle
{
    public class GameEventArgs : EventArgs
    {
        public RunGames Game { get; set; }
        public object Param { get; set; }

        public GameEventArgs(RunGames game, object param)
        {
            game = Game;
            Param = param;
        }
    }

    public abstract class GameScreen
    {
        /// <summary>
        /// Represents the device which draws the screen.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Represents the windows inside which the game is rendered.
        /// </summary>
        public GameWindow Window { get; private set; }

        /// <summary>
        /// Object which manages game content: sprites, textures, models, sound, etc.
        /// </summary>
        public ContentManager Content { get; set; }

        /// <summary>
        /// A collection of game's services
        /// </summary>
        public GameServiceContainer Services { get; private set; }

        /// <summary>
        /// Determines whether screen should be drawn.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Determines whether screen should be updated.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Object to which the content will be drawn.
        /// </summary>
        public RenderTarget2D RenderTarget2D { get; set; }

        /// <summary>
        /// An object which dows the 2D drawing.
        /// All drawing has to come between Begin and End method calls.
        /// </summary>
        protected SpriteBatch spriteBatch;

        protected TimeSpan startingTime;

        public GameScreen(Game game)
        {
            GraphicsDevice = game.GraphicsDevice;
            Content = game.Content;
            Window = game.Window;
            Services = game.Services;

            Enabled = true;
            Visible = true;
        }

        /// <summary>
        /// Allows the screen to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// LoadContent will be called once per screen and is the place to load
        /// all of your content.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// UnloadContent will be called once per screen and is the place to unload
        /// game-specific content.
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Allows the screen to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Draw(GameTime gameTime) { }

        public void SetStartingTime(TimeSpan time)
        {
            startingTime = time;
        }
    }
}
