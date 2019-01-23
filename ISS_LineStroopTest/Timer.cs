using System;
using Microsoft.Xna.Framework;

namespace ISS_NBackCircle
{
    public class Timer
    {
        /// <summary>
        /// Defines how long (in seconds) the timer will run
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Defines whether timer will automatically reset itself upon completition
        /// </summary>
        public bool IsLooping { get; set; } = false;

        /// <summary>
        /// Specifies how many seconds have passed since timer started
        /// </summary>
        public double ElapsedTime { get; private set; } = 0d;

        /// <summary>
        /// Specifies whether timer has finished
        /// </summary>
        public bool IsComplete => ElapsedTime >= Time;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler OnTimerComplete;

        /// <summary>
        /// Creates a new timer
        /// </summary>
        /// <param name="time">How long the timer will run (in seconds)</param>
        public Timer(double time)
        {
            Time = time;
        }

        /// <summary>
        /// Updates the timer
        /// </summary>
        /// <param name="gameTime">The <see cref="GameTime"/> of the game</param>
        public void Update(GameTime gameTime)
        {
            if (ElapsedTime < Time)
                ElapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                OnTimerComplete?.Invoke(this, new EventArgs());
                if (IsLooping)
                    ElapsedTime -= Time;
            }
        }

        /// <summary>
        /// Restarts timer
        /// </summary>
        public void Reset()
        {
            ElapsedTime = 0;
        }
    }
}
