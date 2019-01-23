using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;

namespace ISS_NBackCircle
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont _font;
        Vector2 _timePosition;

        RenderTarget2D _continuousRender;
        RenderTarget2D _discreteRender;

        Rectangle _continuousSize;
        Rectangle _discreteSize;

        Config _config;
        List<GameScreen> _screens;

        bool _isGameActive;
        double _gameElapsed;

        bool _isBeginning;
        double _beginningElapsed;

        Texture2D _backgroundContinuous;
        Texture2D _backgroundDiscrete;
        Texture2D _textReady;
        Texture2D _textGo;
        Texture2D _textTimeUp;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _isGameActive = false;
            _gameElapsed = 0.0d;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Učitaj konfiguraciju
            LoadConfiguration(out _config);

            // Podesi konfiguraciju
            graphics.PreferredBackBufferWidth = _config.Width;
            graphics.PreferredBackBufferHeight = _config.Height;
            graphics.IsFullScreen = _config.Fullscreen;
            graphics.ApplyChanges();

            //Inicijaliziraj scene
            if (_config.RunGames == RunGames.Both)
            {
                if (_config.LeftSideGame == RunGames.Continuous)
                    _config.DiscreteConfig.IsRight = true;
                else
                    _config.ContinuousConfig.IsRight = true;
            }

            _screens = new List<GameScreen>();

            if (_config.RunGames == RunGames.Continuous || _config.RunGames == RunGames.Both)
                _screens.Add(new ContinuousGame(this, _config.ContinuousConfig, _config.GameDuration));

            if (_config.RunGames == RunGames.Discrete || _config.RunGames == RunGames.Both)
                _screens.Add(new DiscreteGame(this, _config.DiscreteConfig, _config.GameDuration));

            foreach (var screen in _screens)
                screen.Initialize();

            base.Initialize();
        }

        private void LoadConfiguration(out Config config)
        {
            // Kreiraj konfiguracijske podatke. Vrijednosti će se inicializirati na default.
            config = new Config();
            var serializer = new XmlSerializer(typeof(Config));

            try
            {
                // Učitaj config.xml
                using (var reader = new FileStream(@".\config.xml", FileMode.Open))
                    config = (Config)serializer.Deserialize(reader);
            }
            catch (FileNotFoundException)
            {
                // Kreiraj novi config.xml
                using (var writer = new StreamWriter(@".\config.xml"))
                    serializer.Serialize(writer, config);

                return;
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _font = Content.Load<SpriteFont>("Arial");
            _timePosition = new Vector2(_config.Width / 2, 10 + _font.MeasureString("MEASURESTRING").Y / 2);

            _backgroundContinuous = Content.Load<Texture2D>(@"Josip\bg_green");
            _backgroundDiscrete = Content.Load<Texture2D>(@"Josip\bg_wood");
            _textReady = Content.Load<Texture2D>(@"Josip\text_ready");
            _textGo = Content.Load<Texture2D>(@"Josip\text_go");
            _textTimeUp = Content.Load<Texture2D>(@"Josip\text_timeup");

            int width = _config.Width;
            int height = _config.Height;

            int continuousWidth = 0;
            int discreteWidth = 0;

            if (_config.RunGames != RunGames.Discrete)
            {
                continuousWidth = _config.RunGames == RunGames.Both ? (int)Math.Round(width * _config.LeftScreenRelativeWidth) : width;
            }

            discreteWidth = (continuousWidth != 0) ? width - continuousWidth : width;

            if (_config.RunGames == RunGames.Both && _config.LeftSideGame == RunGames.Discrete)
            {
                var helper = continuousWidth;
                continuousWidth = discreteWidth;
                discreteWidth = helper;
            }

            if (_config.RunGames == RunGames.Both || _config.RunGames == RunGames.Continuous)
                _continuousRender = new RenderTarget2D(GraphicsDevice, continuousWidth, height);

            if (_config.RunGames == RunGames.Both || _config.RunGames == RunGames.Discrete)
                _discreteRender = new RenderTarget2D(GraphicsDevice, discreteWidth, height);

            foreach (var screen in _screens)
            {
                if (screen is ContinuousGame)
                    screen.RenderTarget2D = _continuousRender;

                if (screen is DiscreteGame)
                    screen.RenderTarget2D = _discreteRender;
            }

            if (_config.RunGames == RunGames.Both)
            {
                if (_config.LeftSideGame == RunGames.Continuous)
                {
                    _continuousSize = new Rectangle(0, 0, continuousWidth, height);
                    _discreteSize = new Rectangle(continuousWidth, 0, discreteWidth, height);
                } else
                {
                    _discreteSize = new Rectangle(0, 0, discreteWidth, height);
                    _continuousSize = new Rectangle(discreteWidth, 0, continuousWidth, height);
                }
                
            } else if (_config.RunGames == RunGames.Continuous)
            {
                _continuousSize = new Rectangle(0, 0, continuousWidth, height);
            }
            else
            {
                _discreteSize = new Rectangle(0, 0, discreteWidth, height);
            }

            foreach (var screen in _screens)
                screen.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (var screen in _screens)
                screen.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            if (_isGameActive)
            {
                _gameElapsed += gameTime.ElapsedGameTime.TotalSeconds;

                foreach (var screen in _screens)
                    if (screen.Enabled)
                        screen.Update(gameTime);

                if (_gameElapsed > _config.GameDuration + 0.01d)
                {
                    _isGameActive = false;
                }
            }
            else
            {
                if (_isBeginning)
                {
                    _beginningElapsed += gameTime.ElapsedGameTime.TotalSeconds;

                    if (_beginningElapsed > 1.5d)
                    {
                        foreach (var screen in _screens)
                            screen.SetStartingTime(gameTime.TotalGameTime);
                        _gameElapsed = 0.0d;
                        _isGameActive = true;
                        _isBeginning = false;
                    }
                }
                else
                {
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
                    {
                        if (_gameElapsed > _config.GameDuration)
                        {
                            Exit();
                            return;
                        }

                        _beginningElapsed = 0d;
                        _isBeginning = true;
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.TransparentBlack);
            var text = "";
            var rect = new Rectangle(GraphicsDevice.Viewport.Bounds.Center, new Point(0, 0));

            if (!_isGameActive && _gameElapsed < _config.GameDuration)
            {
                spriteBatch.Begin();

                if (_config.RunGames == RunGames.Continuous || _config.RunGames == RunGames.Both)
                    DrawContinuousBackground();

                if (_config.RunGames == RunGames.Discrete || _config.RunGames == RunGames.Both)
                    DrawDiscreteBackground();

                if (_isBeginning)
                {                  

                    if (_beginningElapsed < 1d)
                    {
                        rect.X -= _textReady.Width / 2;
                        rect.Y -= _textReady.Height / 2;
                        rect.Width = _textReady.Width;
                        rect.Height = _textReady.Height;
                        spriteBatch.Draw(_textReady, rect, Color.White);
                    }
                    else
                    {
                        rect.X -= _textGo.Width / 2;
                        rect.Y -= _textGo.Height / 2;
                        rect.Width = _textGo.Width;
                        rect.Height = _textGo.Height;
                        spriteBatch.Draw(_textGo, rect, Color.White);

                    }
                }
                else
                {
                    text = "Press ENTER to start";
                    spriteBatch.DrawString(_font, text, GraphicsDevice.Viewport.Bounds.Center.ToVector2() - _font.MeasureString(text) / 2, Color.LightBlue);
                }

                spriteBatch.End();

                return;
            }

            foreach (var screen in _screens)
            {
                if (screen is ContinuousGame)
                    GraphicsDevice.SetRenderTarget(_continuousRender);

                if (screen is DiscreteGame)
                    GraphicsDevice.SetRenderTarget(_discreteRender);

                if (screen.Visible)
                    screen.Draw(gameTime);
            }

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();

            if (_config.RunGames == RunGames.Continuous || _config.RunGames == RunGames.Both)
                spriteBatch.Draw(_continuousRender, _continuousSize, Color.White);

            if (_config.RunGames == RunGames.Discrete || _config.RunGames == RunGames.Both)
                spriteBatch.Draw(_discreteRender, _discreteSize, Color.White);

            var remaining = (int)(_config.GameDuration - _gameElapsed) + 1;
            text = "Time: " + remaining.ToString();

            if (_gameElapsed < _config.GameDuration)
                spriteBatch.DrawString(_font, text, _timePosition - _font.MeasureString(text) / 2, remaining <= 5 ? Color.Yellow : Color.White);

            if (!_isGameActive && _gameElapsed >= _config.GameDuration)
            {
                rect.X -= _textTimeUp.Width / 2;
                rect.Y -= _textTimeUp.Height / 2;
                rect.Width = _textTimeUp.Width;
                rect.Height = _textTimeUp.Height;
                spriteBatch.Draw(_textTimeUp, rect, Color.White);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }

        void DrawContinuousBackground()
        {
            var rect = new Rectangle(_continuousSize.X, _continuousSize.Y, _backgroundContinuous.Width / 2, _backgroundContinuous.Height / 2);

            do
            {
                rect.Y = _continuousSize.Y;

                do
                {
                    spriteBatch.Draw(_backgroundContinuous, rect, Color.LightGray);
                    rect.Y += rect.Height;

                } while (rect.Top < _continuousSize.Bottom);

                rect.X += rect.Width;

            } while (rect.Left < _continuousSize.Right);
        }

        void DrawDiscreteBackground()
        {
            var rect = new Rectangle(_discreteSize.X, _discreteSize.Y, _backgroundDiscrete.Width / 2, _backgroundDiscrete.Height / 2);

            do
            {
                rect.Y = _continuousSize.Y;

                do
                {
                    spriteBatch.Draw(_backgroundDiscrete, rect, Color.DimGray);
                    rect.Y += rect.Height;

                } while (rect.Top < _discreteSize.Bottom);

                rect.X += rect.Width;

            } while (rect.Left < _discreteSize.Right);
        }
    }
}
