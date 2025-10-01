using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace TexturedCube
{
    class Program
    {
        public static void Main(string[] args)
        {
            NativeWindowSettings nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Textured Cube OpenTK"
            };
            using (Game game = new Game(nativeSettings))
            {
                game.Run();
            }
            ;
        }
    }
}