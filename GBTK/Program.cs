using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace GBTK
{
    class Program
    {
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "GBSharp",
            };

            using (FrontEnd frontEnd = new FrontEnd(GameWindowSettings.Default, nativeWindowSettings))
            {
                frontEnd.Run();
            }
        }
    }
}
