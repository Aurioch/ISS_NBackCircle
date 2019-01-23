using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ISS_NBackCircle
{
    public class DiscreteGame : GameScreen
    {
        private List<string> words = new List<string>();
        private int numOfWords;
        private int wordIndex;
        NQueue lastNWords;

        private SpriteFont fontNormal;
        private SpriteFont fontBig;

        private Color answerColor;
        private Color timerColor;

        Random random;

        // Answers stats
        private int correctAnswers;
        private int wrongAnswers;
        private int missedCorrectAnswers;

        private float elapsedTime;
        private bool isSpacePressed;
        private float alpha;
        private bool gameFinished;

        private int N;
        private int wordDuration;
        private float answerDuration;
        private float animationDuration;
        private double gameDuration;
        private Difficulty difficulty;

        bool _nRight;
        int? _seed;
        Texture2D _background;

        public DiscreteGame(Game game, DiscreteConfig config, double duration) : base(game)
        {
            N = config.N;
            wordDuration = config.wordDuration;
            answerDuration = config.answerDuration;
            animationDuration = config.animationDuration;
            gameDuration = duration;
            difficulty = config.difficulty;

            _nRight = config.WriteNRight;
            _seed = config.Seed;
        }

        public override void Initialize()
        {
            // load words

            string fileName = null;
            if (difficulty == Difficulty.Easy)
            {
                fileName = "Robert/Words - Easy.txt";
            } else if (difficulty == Difficulty.Medium)
            {
                fileName = "Robert/Words - Medium.txt";
            }
            else
            {
                fileName = "Robert/Words - Hard.txt";
            }

            using (var stream = TitleContainer.OpenStream(fileName))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        words.Add(reader.ReadLine());
                    }
                }
            }

            if (_seed.HasValue)
                random = new Random(_seed.Value);
            else
                random = new Random();

            numOfWords = words.Count;
            wordIndex = random.Next(numOfWords);
            lastNWords = new NQueue(N);

            answerColor = Color.White;
            timerColor = Color.GreenYellow;

            elapsedTime = (float)wordDuration;
            gameFinished = false;
        }

        public override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fontNormal = Content.Load<SpriteFont>("Arial");
            fontBig = Content.Load<SpriteFont>("Arial-60");

            _background = Content.Load<Texture2D>(@"Josip\bg_wood");
        }

        public override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (gameFinished)
                return;

            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Space) && !isSpacePressed)
            {
                isSpacePressed = true;
                updateScore();                
                return;
            } 

            elapsedTime -= (float)gameTime.ElapsedGameTime.TotalSeconds; 
            if (elapsedTime <= 0.0f)
            {
                // check for missed correct answers
                if (!isSpacePressed)
                {
                    if (updateScore())
                        return;
                }

                lastNWords.Add(words[wordIndex]);

                // get a new word, there is 25% chance to intentionally generate correct answer
                var rand = random.NextDouble();
                if (rand >= 0.75)
                {
                    Debug.WriteLine("tu sam");
                    wordIndex = words.IndexOf(lastNWords.Peek());
                    
                } else
                {
                    wordIndex = random.Next(numOfWords);
                    Debug.WriteLine(wordIndex);
                }

                // reset
                answerColor = Color.White;
                isSpacePressed = false;
                elapsedTime = (float)wordDuration;
            }

            // fade in animation
            var diff = elapsedTime - (wordDuration - animationDuration);
            if (diff >= 0.0f)
            {
                alpha = 1 - diff/animationDuration;
            }

            gameDuration -= gameTime.ElapsedGameTime.TotalSeconds;
            if (gameDuration <= 5.0d)
            {
                timerColor = Color.Red;
                if (gameDuration <= 0.0d)
                    gameFinished = true;
            } 
        }

        public override void Draw(GameTime gameTime)
        {
            int height = GraphicsDevice.Viewport.Height;
            int width = GraphicsDevice.Viewport.Width;

            int defaultMargin = 20;
            int rightMargin = width - 200;
            Vector2 wordPosition;

            spriteBatch.Begin();

            var rect = new Rectangle(0, 0, _background.Width / 2, _background.Height / 2);

            do
            {
                rect.Y = 0;

                do
                {
                    spriteBatch.Draw(_background, rect, Color.DimGray);
                    rect.Y += rect.Height;

                } while (rect.Top < RenderTarget2D.Bounds.Height);

                rect.X += rect.Width;

            } while (rect.Left < RenderTarget2D.Bounds.Width);

            if (!gameFinished)
            {
                spriteBatch.DrawString(
                    fontNormal, "Score: " +
                    (correctAnswers - wrongAnswers - missedCorrectAnswers).ToString(),
                    new Vector2(rightMargin, defaultMargin), Color.GreenYellow
                );
                //spriteBatch.DrawString(fontNormal, "Time: " + (int)gameDuration +" s", new Vector2(defaultMargin, 3 * defaultMargin + 10), timerColor);
                spriteBatch.DrawString(fontNormal, "N = " + N, new Vector2(_nRight ? rightMargin + 55 : defaultMargin, defaultMargin + (_nRight ? fontNormal.MeasureString("N = ").Y + 10 : 0)), Color.GreenYellow);
                wordPosition = calculateWordPosition(width, height, null);
                spriteBatch.DrawString(fontBig, words[wordIndex], wordPosition, answerColor * alpha);
            } else
            {
                string score = "Score: " + (correctAnswers - wrongAnswers - missedCorrectAnswers).ToString();
                wordPosition = calculateWordPosition(width, height, score);
                spriteBatch.DrawString(fontNormal, score, new Vector2(wordPosition.X, 100), Color.CornflowerBlue);

                string correctAnswersString = "Correct answers: " + correctAnswers.ToString();
                wordPosition = calculateWordPosition(width, height, correctAnswersString);
                spriteBatch.DrawString(fontNormal, correctAnswersString, new Vector2(wordPosition.X, 200), Color.Green);

                string wrongAnswersString = "Wrong answers: " + wrongAnswers.ToString();
                wordPosition = calculateWordPosition(width, height, wrongAnswersString);
                spriteBatch.DrawString(fontNormal, wrongAnswersString, new Vector2(wordPosition.X, 250), Color.Red);

                string missedAnswersString = "Missed answers: " + missedCorrectAnswers.ToString();
                wordPosition = calculateWordPosition(width, height, missedAnswersString);
                spriteBatch.DrawString(fontNormal, missedAnswersString, new Vector2(wordPosition.X, 300), Color.Orange);
            }

            spriteBatch.End();
        }

        private bool isAnswerCorrect()
        {
            return lastNWords.isMaxCapacity() && words[wordIndex].Equals(lastNWords.Peek());
        }

        // returns true/false whether score has been updated or not
        private bool updateScore()
        {
            bool scoreUpdated = false;
            if (isAnswerCorrect() && isSpacePressed)
            {
                scoreUpdated = true;
                answerColor = Color.Green;
                correctAnswers++;
            }
            else if((isAnswerCorrect() && !isSpacePressed))
            {
                scoreUpdated = true;
                answerColor = Color.Red;
                missedCorrectAnswers++;
            } else if((!isAnswerCorrect() && isSpacePressed))
            {
                scoreUpdated = true;
                answerColor = Color.Red;
                wrongAnswers++;
            }

            if (scoreUpdated)
            {
                alpha = 1.0f;
                // Simulate key pressing to prevent infinite loop
                isSpacePressed = true;
                elapsedTime = answerDuration;
                return true;
            }
            return false;
        }

        private Vector2 calculateWordPosition(int width, int height, string s)
        {
            // Center text
            Vector2 stringSize;
            if (s != null)
            {
                stringSize = fontNormal.MeasureString(s);
            } else
            {
                stringSize = fontBig.MeasureString(words[wordIndex]);
            }
            return new Vector2(width / 2 - stringSize.X/2, height / 2 - stringSize.Y/2);
        }

        public class NQueue : Queue<string>
        {
            int maxSize;
            public NQueue(int max)
            {
                maxSize = max;
            }

            public void Add(string s)
            {
                if (this.Count >= maxSize)
                    this.Dequeue();

                this.Enqueue(s);
            }

            public bool isMaxCapacity()
            {
                return maxSize == this.Count;
            }
        }
    }
}