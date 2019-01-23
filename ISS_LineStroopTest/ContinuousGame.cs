using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ISS_NBackCircle
{
    public class ContinuousMeasurements
    {
        public TimeSpan Time { get; set; }
        public float Delta { get; set; }
        public bool DirectionChange { get; set; }
    }

    public class ContinuousGame : GameScreen
    {
        Rectangle _circlePosition;
        Rectangle _playerPosition;

        Vector2 _circleDirection;
        Vector2 _playerDirection;

        Texture2D _circleTexture;
        Texture2D _playerTexture;
        Texture2D _background;

        Random _r;
        Timer _timer;

        float _circleSpeed;
        float _playerSpeed;

        double _minTime;
        double _maxTime;

        int? _seed;
        List<ContinuousMeasurements> _measurements;

        bool _resultsExported;
        double _elapsed;
        double _duration;

        float _maxDelta;
        int _timesOutside;
        double _wander;
        SpriteFont _font;

        Vector2 DesignResolution = new Vector2(800, 480);
        const int CircleRadius = 32;
        const int PlayerSize = 16;

        public ContinuousGame(Game game, ContinuousConfig config, double duration) : base(game)
        {
            _circleSpeed = config.CircleSpeed;
            _playerSpeed = config.CircleSpeed * config.PlayerRelativeSpeed;
            _minTime = config.CircleMinimumMovementTime;
            _maxTime = config.CircleMaximumMovementTime;
            _seed = config.Seed;

            _duration = duration;
            _elapsed = 0d;
        }

        public override void Initialize()
        {
            if (_seed.HasValue)
                _r = new Random(_seed.Value);
            else
                _r = new Random();

            _timer = new Timer(_minTime + (_maxTime - _minTime) * _r.NextDouble());
            _playerDirection = new Vector2(0, 0);
            _measurements = new List<ContinuousMeasurements>();
            _resultsExported = false;
        }

        public override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _circleTexture = Content.Load<Texture2D>(@"Josip/target_red2_outline");
            _playerTexture = Content.Load<Texture2D>(@"Josip/crosshair_outline_large");
            _background = Content.Load<Texture2D>(@"Josip/bg_green");
            _font = Content.Load<SpriteFont>("Arial");

            float heightMultiplier = GraphicsDevice.PresentationParameters.BackBufferHeight / DesignResolution.Y;
            int circleSize = (int)Math.Round(2 * CircleRadius * heightMultiplier);
            int playerSize = (int)Math.Round(PlayerSize * heightMultiplier);

            _circlePosition = new Rectangle(_r.Next(0, RenderTarget2D.Width - circleSize), _r.Next(0, RenderTarget2D.Height - circleSize), circleSize, circleSize);
            _playerPosition = new Rectangle(_circlePosition.Center.X - playerSize / 2, _circlePosition.Center.Y - playerSize / 2, playerSize, playerSize);

            _circleDirection = new Vector2((float)_r.NextDouble() - 0.5f, (float)_r.NextDouble() - 0.5f);
            _circleDirection.Normalize();
        }

        public override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            _timer.Update(gameTime);
            _elapsed += gameTime.ElapsedGameTime.TotalSeconds;

            _playerDirection.X = 0;
            _playerDirection.Y = 0;

            if (keyboard.IsKeyDown(Keys.Left) && _playerPosition.Left > 0)
                _playerDirection.X = -1;

            if (keyboard.IsKeyDown(Keys.Right) && _playerPosition.Right < RenderTarget2D.Width)
                _playerDirection.X = 1;

            if (keyboard.IsKeyDown(Keys.Up) && _playerPosition.Top > 0)
                _playerDirection.Y = -1;

            if (keyboard.IsKeyDown(Keys.Down) && _playerPosition.Bottom < RenderTarget2D.Height)
                _playerDirection.Y = 1;

            _playerPosition.X += (int)Math.Round(_playerDirection.X * _playerSpeed);
            _playerPosition.Y += (int)Math.Round(_playerDirection.Y * _playerSpeed);

            _circlePosition.X += (int)Math.Round(_circleDirection.X * _circleSpeed);
            _circlePosition.Y += (int)Math.Round(_circleDirection.Y * _circleSpeed);

            var wasChange = false;
            if (_timer.IsComplete || !RenderTarget2D.Bounds.Contains(_circlePosition))
            {
                wasChange = true;

                if (_circlePosition.Left <= 0)
                    _circleDirection = new Vector2((float)_r.NextDouble() * 0.5f, (float)_r.NextDouble() - 0.5f);
                else if (_circlePosition.Right >= RenderTarget2D.Width)
                    _circleDirection = new Vector2((float)_r.NextDouble() * (-0.5f), (float)_r.NextDouble() - 0.5f);
                else if (_circlePosition.Top <= 0)
                    _circleDirection = new Vector2((float)_r.NextDouble() - 0.5f, (float)_r.NextDouble() * 0.5f);
                else if (_circlePosition.Bottom >= RenderTarget2D.Height)
                    _circleDirection = new Vector2((float)_r.NextDouble() - 0.5f, (float)_r.NextDouble() * (-0.5f));
                else
                    _circleDirection = new Vector2((float)_r.NextDouble() - 0.5f, (float)_r.NextDouble() - 0.5f);
                _circleDirection.Normalize();
                _timer.Reset();
            }

            _measurements.Add(new ContinuousMeasurements()
            {
                Time = gameTime.TotalGameTime,
                Delta = Vector2.Distance(_circlePosition.Center.ToVector2(), _playerPosition.Center.ToVector2()),
                DirectionChange = wasChange
            });

            if (!_resultsExported && _elapsed >= _duration)
                ExportCSV();
        }

        private void ExportCSV()
        {
            var lines = new List<string>();
            _maxDelta = 0f;
            _timesOutside = 0;
            _wander = 0d;

            TimeSpan timeSpan = TimeSpan.Zero;

            for (int i = 0; i < _measurements.Count; i++)
            {
                if (_measurements[i].Delta > _maxDelta)
                    _maxDelta = _measurements[i].Delta;

                if (_measurements[i].Delta > _circlePosition.Width / 2 && _measurements[i - 1].Delta <= _circlePosition.Width / 2)
                {
                    _timesOutside++;
                    timeSpan = _measurements[i].Time;
                }

                if (i > 0 &&_measurements[i].Delta <= _circlePosition.Width / 2 && _measurements[i - 1].Delta > _circlePosition.Width / 2)
                    _wander += (_measurements[i].Time - timeSpan).TotalSeconds;

                lines.Add((_measurements[i].Time - startingTime).ToString() + ";" + _measurements[i].Delta.ToString() + ";" + _measurements[i].DirectionChange.ToString());
            }

            try
            {
                File.WriteAllLines(@".\continuousResults.csv", lines);
            }
            catch (IOException e)
            {
                // Gutanje exceptiona je loše. U ovom slučaju se može istrpiti.
            }

            _resultsExported = true;
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.TransparentBlack);

            spriteBatch.Begin();

            var rect = new Rectangle(0, 0, _background.Width / 2, _background.Height / 2);

            do
            {
                rect.Y = 0;

                do
                {
                    spriteBatch.Draw(_background, rect, Color.LightGray);
                    rect.Y += rect.Height;

                } while (rect.Top < RenderTarget2D.Bounds.Right);

                rect.X += rect.Width;

            } while (rect.Left < RenderTarget2D.Bounds.Bottom);

            spriteBatch.Draw(_circleTexture, _circlePosition, Color.Gray);
            spriteBatch.Draw(_playerTexture, _playerPosition, Color.White);

            if (_elapsed >= _duration)
            {
                var text = "Left circle: " + _timesOutside.ToString() + " times";
                var position = new Vector2(RenderTarget2D.Width / 2, RenderTarget2D.Height / 8);
                spriteBatch.DrawString(_font, text, position - _font.MeasureString(text) / 2, Color.LightBlue);

                text = "Remained outside: " + Math.Round(_wander, 2).ToString() + " seconds";
                position.Y += _font.MeasureString(text).Y + 5;
                spriteBatch.DrawString(_font, text, position - _font.MeasureString(text) / 2, Color.LightBlue);

                text = "Max delta: " + _maxDelta.ToString();
                position.Y += _font.MeasureString(text).Y + 5;
                spriteBatch.DrawString(_font, text, position - _font.MeasureString(text) / 2, Color.LightBlue);
            }

            spriteBatch.End();
        }
    }
}
