namespace ISS_NBackCircle
{
    public class Config
    {
        public int Width { get; set; } = 1366;
        public int Height { get; set; } = 768;
        public bool Fullscreen { get; set; } = false;

        public RunGames RunGames { get; set; } = RunGames.Both;
        public float LeftScreenRelativeWidth { get; set; } = 768f / 1366;
        public RunGames LeftSideGame { get; set; } = RunGames.Continuous;
        public double GameDuration { get; set; } = 60.0d;

        public ContinuousConfig ContinuousConfig { get; set; } = new ContinuousConfig();
        public DiscreteConfig DiscreteConfig { get; set; } = new DiscreteConfig();
    }

    public class ContinuousConfig
    {
        public int? Seed { get; set; } = null;
        public bool UseMouse { get; set; } = false;
        public float CircleSpeed { get; set; } = 2.0f;
        public float PlayerRelativeSpeed { get; set; } = 1.5f;
        public double CircleMinimumMovementTime { get; set; } = 0.5d;
        public double CircleMaximumMovementTime { get; set; } = 1.5d;

        [System.Xml.Serialization.XmlIgnore]
        public bool IsRight { get; set; } = false;
    }

    public class DiscreteConfig
    {
        public int? Seed { get; set; } = null;
        public int N { get; set; } = 2;
        public int wordDuration { get; set; } = 5;
        public float answerDuration { get; set; } = 0.75f;
        public float animationDuration { get; set; } = 0.75f;
        public Difficulty difficulty { get; set; } = Difficulty.Easy;

        [System.Xml.Serialization.XmlIgnore]
        public bool IsRight { get; set; } = false;
    }

    public enum RunGames
    {
        Continuous,
        Discrete,
        Both
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
}