using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace Cube
{
    class Program
    {
        static void Main()
        {
            var nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 600),
                Title = "OpenTK Cube (Mac)"
            };
            using (var window = new Game(nativeSettings))
            {
                window.Run();
            }
        }
    }
}
