namespace _2DCollision
{
    class Program
    {
        public static void Main(string[] args)
        {
            using BreakoutGame game = new((800, 600), "Breakout (OpenTK)");
            game.Run();
        }
    }
}