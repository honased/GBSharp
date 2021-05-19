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
                Title = "GBSharp"
            };

            var gws = new GameWindowSettings() { IsMultiThreaded = false, RenderFrequency = 60, UpdateFrequency = 60 };

            using (Window frontEnd = new Window(gws, nativeWindowSettings))
            {
                frontEnd.Run();
            }
        }
    }
}
